import { useEffect, useState } from 'react';
import { BrowserRouter as Router, Routes, Route, useNavigate, useLocation } from 'react-router-dom';
import * as XLSX from 'xlsx';
import { ContactsApi } from './api';
import {
    Table,
    Button,
    Form,
    Input,
    Checkbox,
    Space,
    Modal,
    message,
    Card,
    Row,
    Col,
    Typography,
    Divider,
    Popconfirm,
    Layout,
    Menu,
    Switch,
    Statistic,
    Upload
} from 'antd';
import { PlusOutlined, EditOutlined, DeleteOutlined, SearchOutlined, HomeOutlined, ContactsOutlined, CheckOutlined, CloseOutlined, UploadOutlined, DownloadOutlined, SaveOutlined } from '@ant-design/icons';

const { Title } = Typography;
const { Search } = Input;
const { Header, Content } = Layout;

function ContactsManager() {
    const [contacts, setContacts] = useState([]);
    const [filteredContacts, setFilteredContacts] = useState([]);
    const [loading, setLoading] = useState(false);
    const [modalVisible, setModalVisible] = useState(false);
    const [editingContact, setEditingContact] = useState(null);
    const [searchTerm, setSearchTerm] = useState('');
    const [filterStatus, setFilterStatus] = useState('all'); // 'all', 'used', 'unused'
    const [form] = Form.useForm();
    const navigate = useNavigate();
    const location = useLocation();

    const load = async () => {
        setLoading(true);
        try {
            const data = await ContactsApi.list();
            setContacts(data);
            filterContacts(data, searchTerm, filterStatus);
        } catch (error) {
            message.error('Failed to load contacts: ' + error.message);
        } finally {
            setLoading(false);
        }
    };

    useEffect(() => {
        load();
    }, []);

    const filterContacts = (contactsList, search, status) => {
        let filtered = contactsList.filter(c => {
            const matchesSearch = search === '' ||
                c.firstName.toLowerCase().includes(search.toLowerCase()) ||
                c.lastName.toLowerCase().includes(search.toLowerCase()) ||
                c.phone.includes(search);

            const matchesFilter = status === 'all' ||
                (status === 'used' && c.used) ||
                (status === 'unused' && !c.used);

            return matchesSearch && matchesFilter;
        });

        setFilteredContacts(filtered);
    };

    useEffect(() => {
        filterContacts(contacts, searchTerm, filterStatus);
    }, [contacts, searchTerm, filterStatus]);

    const handleAdd = () => {
        setEditingContact(null);
        form.resetFields();
        setModalVisible(true);
    };

    const handleEdit = (contact) => {
        setEditingContact(contact);
        form.setFieldsValue(contact);
        setModalVisible(true);
    };

    const handleDelete = async (id) => {
        try {
            await ContactsApi.remove(id);
            message.success('Contact deleted successfully');
            load();
        } catch (error) {
            message.error('Failed to delete contact: ' + error.message);
        }
    };

    const handleSubmit = async (values) => {
        try {
            if (editingContact) {
                await ContactsApi.update(editingContact.id, values);
                message.success('Contact updated successfully');
            } else {
                await ContactsApi.create(values);
                message.success('Contact added successfully');
            }
            setModalVisible(false);
            load();
        } catch (error) {
            message.error('Failed to save contact: ' + error.message);
        }
    };

    const toggleUsed = async (contact) => {
        try {
            await ContactsApi.update(contact.id, { ...contact, used: !contact.used });
            message.success(`Contact marked as ${!contact.used ? 'used' : 'unused'}`);
            load();
        } catch (error) {
            message.error('Failed to update contact: ' + error.message);
        }
    };

    const handleImport = (file) => {
        const reader = new FileReader();
        reader.onload = async (e) => {
            try {
                const data = new Uint8Array(e.target.result);
                const workbook = XLSX.read(data, { type: 'array' });
                const sheetName = workbook.SheetNames[0];
                const worksheet = workbook.Sheets[sheetName];
                const jsonData = XLSX.utils.sheet_to_json(worksheet);

                if (jsonData.length === 0) {
                    message.warning('The Excel file appears to be empty');
                    return;
                }

                console.log('Parsed Excel data:', jsonData);
                console.log('First row keys:', Object.keys(jsonData[0] || {}));
                console.log('First row data sample:', jsonData[0]);

                let importedCount = 0;
                let duplicateCount = 0;
                let errorCount = 0;

                for (const row of jsonData) {
                    try {
                        console.log('Processing row:', row);

                        // Try to map common column names
                        const firstName = row['Name'] || row['First Name'] || row['FirstName'] || row['first_name'] || row['firstname'] || '';
                        const lastName = row['Surname'] || row['Last Name'] || row['LastName'] || row['last_name'] || row['lastname'] || '';
                        const phone = row['Telephone Number'] || row['Phone Number'] || row['Phone'] || row['PhoneNumber'] || row['phone_number'] || row['phone'] || row['Cell Phone'] || row['Mobile'] || row['Telephone'] || '';
                        // Default "used" to false if column doesn't exist, handle various boolean formats
                        const usedValue = row['Used'] || row['used'] || false;
                        let used = false;

                        if (typeof usedValue === 'boolean') {
                            used = usedValue;
                        } else if (typeof usedValue === 'string') {
                            const lowerValue = usedValue.toLowerCase().trim();
                            used = lowerValue === 'true' || lowerValue === 'yes' || lowerValue === '1';
                        } else if (typeof usedValue === 'number') {
                            used = usedValue === 1;
                        }

                        console.log('Mapped values:', { firstName, lastName, phone, used });

                        if (!firstName && !lastName && !phone) {
                            continue; // Skip completely empty rows
                        }

                        // Only require firstName, lastName, and phone - "used" is optional
                        if (!firstName || !lastName || !phone) {
                            console.log('Skipping row due to missing required fields:', { name: firstName, surname: lastName, phone });
                            errorCount++;
                            continue;
                        }

                        // Check for duplicates (same phone number)
                        const existingContact = contacts.find(c => c.phone === phone);
                        if (existingContact) {
                            duplicateCount++;
                            continue;
                        }

                        // Create new contact with "used" defaulting to false
                        const newContact = {
                            firstName: firstName.toString().trim(),
                            lastName: lastName.toString().trim(),
                            phone: phone.toString().trim(),
                            used: used
                        };

                        console.log('Importing contact:', newContact);
                        await ContactsApi.create(newContact);
                        importedCount++;

                    } catch (error) {
                        console.error('Error importing row:', error);
                        errorCount++;
                    }
                }

                // Reload contacts to show imported data
                await load();

                // Show summary message
                let summaryMessage = `Import completed: ${importedCount} contacts imported`;
                if (duplicateCount > 0) {
                    summaryMessage += `, ${duplicateCount} duplicates skipped`;
                }
                if (errorCount > 0) {
                    summaryMessage += `, ${errorCount} errors`;
                }

                if (importedCount > 0) {
                    message.success(summaryMessage);
                } else if (duplicateCount > 0) {
                    message.warning(summaryMessage);
                } else {
                    message.error(summaryMessage);
                }

            } catch (error) {
                message.error('Failed to import file: ' + error.message);
            }
        };
        reader.readAsArrayBuffer(file);
        return false; // Prevent upload
    };

    const handleExport = () => {
        try {
            // Convert contacts to CSV format with semicolon delimiter
            const headers = ['Name', 'Surname', 'Telephone Number', 'Used'];
            const csvContent = [
                headers.join(';'),
                ...contacts.map(contact => {
                    // Add tab character before phone number to force Excel to treat as text and preserve leading zeros
                    const phoneFormatted = `"\t${contact.phone}"`;
                    return [contact.firstName, contact.lastName, phoneFormatted, contact.used ? 'true' : 'false'].join(';');
                })
            ].join('\n');

            const blob = new Blob([csvContent], { type: 'text/csv' });
            const url = window.URL.createObjectURL(blob);
            const link = document.createElement('a');
            link.href = url;

            // Add timestamp to filename
            const now = new Date();
            const timestamp = now.toISOString().replace(/[:.]/g, '-').slice(0, 19); // Format: YYYY-MM-DDTHH-MM-SS
            link.download = `contacts_${timestamp}.csv`;

            link.click();
            window.URL.revokeObjectURL(url);
            message.success('Contacts exported successfully');
        } catch (error) {
            message.error('Failed to export contacts: ' + error.message);
        }
    };

    const handleSaveToDatabase = async () => {
        try {
            await ContactsApi.saveAll(contacts);
            message.success('All contacts saved to database/Excel file successfully!');
        } catch (error) {
            console.error('Error saving contacts:', error);
            message.error('Failed to save contacts to database: ' + error.message);
        }
    };

    const columns = [
        {
            title: 'Name',
            dataIndex: 'firstName',
            key: 'firstName',
            sorter: (a, b) => a.firstName.localeCompare(b.firstName),
        },
        {
            title: 'Surname',
            dataIndex: 'lastName',
            key: 'lastName',
            sorter: (a, b) => a.lastName.localeCompare(b.lastName),
            render: (text, record) => (
                <span
                    style={{
                        backgroundColor: record.used ? '#ffebee' : '#e8f5e8',
                        padding: '4px 8px',
                        borderRadius: '4px',
                        display: 'inline-block',
                        width: '100%'
                    }}
                >
                    {text}
                </span>
            ),
        },
        {
            title: 'Telephone Number',
            dataIndex: 'phone',
            key: 'phone',
            sorter: (a, b) => a.phone.localeCompare(b.phone),
        },
        {
            title: 'Used',
            dataIndex: 'used',
            key: 'used',
            sorter: (a, b) => (a.used ? 1 : 0) - (b.used ? 1 : 0),
            render: (used) => (
                <span style={{ color: used ? '#d32f2f' : '#2e7d32', fontWeight: 'bold' }}>
                    {used ? 'Yes' : 'No'}
                </span>
            ),
        },
        {
            title: 'Actions',
            key: 'actions',
            render: (_, record) => (
                <Space size="small">
                    <Button
                        type="primary"
                        size="small"
                        icon={<EditOutlined />}
                        onClick={() => handleEdit(record)}
                    >
                        Edit
                    </Button>
                    <Switch
                        checked={record.used}
                        onChange={() => toggleUsed(record)}
                        checkedChildren={<CheckOutlined />}
                        unCheckedChildren={<CloseOutlined />}
                        size="small"
                        style={{
                            backgroundColor: record.used ? '#f5222d' : '#52c41a',
                            marginBottom: '2px',
                            marginLeft: '4px',
                            marginRight: '4px',
                        }}
                    />
                    <Popconfirm
                        title="Are you sure you want to delete this contact?"
                        onConfirm={() => handleDelete(record.id)}
                        okText="Yes"
                        cancelText="No"
                    >
                        <Button
                            type="primary"
                            danger
                            size="small"
                            icon={<DeleteOutlined />}
                        >
                            Delete
                        </Button>
                    </Popconfirm>
                </Space>
            ),
        },
    ];

    const menuItems = [
        {
            key: '/',
            icon: <HomeOutlined />,
            label: 'Home',
        },
        {
            key: '/contacts',
            icon: <ContactsOutlined />,
            label: 'Contacts',
        },
    ];

    const handleMenuClick = (e) => {
        navigate(e.key);
    };

    const handleFilterToggle = () => {
        const nextStatus = {
            'all': 'used',
            'used': 'unused',
            'unused': 'all'
        };
        setFilterStatus(nextStatus[filterStatus]);
    };

    const getFilterButtonProps = () => {
        switch (filterStatus) {
            case 'all':
                return {
                    text: 'Show: All',
                    type: 'default',
                    style: { backgroundColor: '#1890ff', color: 'white', borderColor: '#1890ff' }
                };
            case 'used':
                return {
                    text: 'Show: Used',
                    type: 'default',
                    style: { backgroundColor: '#f5222d', color: 'white', borderColor: '#f5222d' }
                };
            case 'unused':
                return {
                    text: 'Show: Unused',
                    type: 'default',
                    style: { backgroundColor: '#52c41a', color: 'white', borderColor: '#52c41a' }
                };
            default:
                return { text: 'Show: All', type: 'default' };
        }
    };

    const usedContacts = contacts.filter(c => c.used).length;
    const unusedContacts = contacts.length - usedContacts;

    const renderHomePage = () => (
        <Card>
            <div style={{ textAlign: 'center', padding: '60px 20px' }}>
                <ContactsOutlined style={{ fontSize: '64px', color: '#1890ff', marginBottom: '24px' }} />
                <Title level={2}>Welcome to Contacts Manager</Title>
                <p style={{ fontSize: '16px', color: '#666', marginBottom: '32px' }}>
                    Manage your contacts efficiently with our easy-to-use interface.
                </p>
                <Button
                    type="primary"
                    size="large"
                    icon={<ContactsOutlined />}
                    onClick={() => navigate('/contacts')}
                >
                    View Contacts
                </Button>
                <div style={{ marginTop: '40px', display: 'flex', justifyContent: 'center', gap: '40px' }}>
                    <div>
                        <Title level={4} style={{ color: '#1890ff' }}>{contacts.length}</Title>
                        <p>Total Contacts</p>
                    </div>
                    <div>
                        <Title level={4} style={{ color: '#52c41a' }}>{usedContacts}</Title>
                        <p>Used Contacts</p>
                    </div>
                    <div>
                        <Title level={4} style={{ color: '#faad14' }}>{unusedContacts}</Title>
                        <p>Unused Contacts</p>
                    </div>
                </div>
            </div>
        </Card>
    );

    const renderContactsPage = () => (
        <Card>
            <Row justify="space-between" align="middle" style={{ marginBottom: '24px' }}>
                <Col>
                    <Space size="middle">
                        <Button
                            type="primary"
                            icon={<PlusOutlined />}
                            onClick={handleAdd}
                            size="large"
                        >
                            Add Contact
                        </Button>
                        <Upload
                            beforeUpload={handleImport}
                            showUploadList={false}
                            accept=".xlsx,.xls,.csv"
                        >
                            <Button
                                icon={<UploadOutlined />}
                                size="large"
                            >
                                Import Excel
                            </Button>
                        </Upload>
                    </Space>
                </Col>
                <Col>
                    <Space size="middle">
                        <Button
                            icon={<SaveOutlined />}
                            onClick={handleSaveToDatabase}
                            size="large"
                            type="default"
                        >
                            Save to Database
                        </Button>
                        <Button
                            icon={<DownloadOutlined />}
                            onClick={handleExport}
                            size="large"
                            type="default"
                        >
                            Export Excel
                        </Button>
                    </Space>
                </Col>
            </Row>

            <Divider />

            <Row gutter={16} style={{ marginBottom: '24px' }}>
                <Col xs={24} sm={12} md={6}>
                    <Search
                        placeholder="Search by name or phone..."
                        allowClear
                        prefix={<SearchOutlined />}
                        value={searchTerm}
                        onChange={(e) => setSearchTerm(e.target.value)}
                        size="large"
                    />
                </Col>
                <Col xs={24} sm={12} md={18}>
                    <div style={{ display: 'flex', alignItems: 'center', gap: '20px', height: '40px' }}>
                        <Button
                            onClick={handleFilterToggle}
                            size="large"
                            {...getFilterButtonProps()}
                        >
                            {getFilterButtonProps().text}
                        </Button>
                        <div style={{ display: 'flex', gap: '20px', alignItems: 'center' }}>
                            <div style={{ display: 'flex', alignItems: 'center', gap: '8px' }}>
                                <span style={{ fontSize: '14px', color: '#1890ff' }}>Total:</span>
                                <span style={{ fontSize: '16px', fontWeight: 'bold', color: '#1890ff' }}>{contacts.length}</span>
                            </div>
                            <div style={{ display: 'flex', alignItems: 'center', gap: '8px' }}>
                                <span style={{ fontSize: '14px', color: '#f5222d' }}>Used:</span>
                                <span style={{ fontSize: '16px', fontWeight: 'bold', color: '#f5222d' }}>{usedContacts}</span>
                            </div>
                            <div style={{ display: 'flex', alignItems: 'center', gap: '8px' }}>
                                <span style={{ fontSize: '14px', color: '#52c41a' }}>Unused:</span>
                                <span style={{ fontSize: '16px', fontWeight: 'bold', color: '#52c41a' }}>{unusedContacts}</span>
                            </div>
                        </div>
                    </div>
                </Col>
            </Row>

            <Table
                columns={columns}
                dataSource={filteredContacts}
                rowKey="id"
                loading={loading}
                pagination={false}
            />
        </Card>
    );

    return (
        <Layout style={{ minHeight: '100vh' }}>
            <Header style={{
                display: 'flex',
                alignItems: 'center',
                backgroundColor: '#1890ff',
                padding: '0 50px'
            }}>
                <div style={{
                    color: 'white',
                    fontSize: '20px',
                    fontWeight: 'bold',
                    marginRight: '50px',
                    display: 'flex',
                    alignItems: 'center',
                    gap: '8px'
                }}>
                    <ContactsOutlined style={{ fontSize: '24px' }} />
                    Contacts Manager
                </div>
                <Menu
                    theme="dark"
                    mode="horizontal"
                    selectedKeys={[location.pathname]}
                    onClick={handleMenuClick}
                    items={menuItems}
                    style={{
                        backgroundColor: 'transparent',
                        borderBottom: 'none',
                        flex: 1
                    }}
                />
            </Header>

            <Content style={{ padding: '24px', maxWidth: '1600px', margin: '0 auto', width: '100%' }}>
                <Routes>
                    <Route path="/" element={renderHomePage()} />
                    <Route path="/contacts" element={renderContactsPage()} />
                </Routes>
            </Content>

            <Modal
                title={editingContact ? 'Edit Contact' : 'Add New Contact'}
                open={modalVisible}
                onCancel={() => setModalVisible(false)}
                footer={null}
                destroyOnClose
            >
                <Form
                    form={form}
                    layout="vertical"
                    onFinish={handleSubmit}
                    initialValues={{ used: false }}
                >
                    <Form.Item
                        name="firstName"
                        label="Name"
                        rules={[{ required: true, message: 'Please enter name!' }]}
                    >
                        <Input placeholder="Enter name" />
                    </Form.Item>

                    <Form.Item
                        name="lastName"
                        label="Surname"
                        rules={[{ required: true, message: 'Please enter surname!' }]}
                    >
                        <Input placeholder="Enter surname" />
                    </Form.Item>

                    <Form.Item
                        name="phone"
                        label="Phone"
                        rules={[
                            { required: true, message: 'Please enter phone number!' },
                            {
                                pattern: /^\+?[0-9]+$/,
                                message: 'Phone number can only contain numbers and an optional + at the beginning!'
                            }
                        ]}
                    >
                        <Input
                            placeholder="Enter phone number (e.g., +1234567890 or 1234567890)"
                            onKeyPress={(e) => {
                                const char = e.key;
                                const value = e.target.value;
                                // Allow + only at the beginning
                                if (char === '+' && value.length > 0) {
                                    e.preventDefault();
                                }
                                // Only allow numbers and + 
                                if (!/[0-9+]/.test(char)) {
                                    e.preventDefault();
                                }
                            }}
                        />
                    </Form.Item>

                    <Form.Item name="used" valuePropName="checked">
                        <Checkbox>Used</Checkbox>
                    </Form.Item>

                    <Form.Item style={{ marginBottom: 0, textAlign: 'right' }}>
                        <Space>
                            <Button onClick={() => setModalVisible(false)}>
                                Cancel
                            </Button>
                            <Button type="primary" htmlType="submit">
                                {editingContact ? 'Update' : 'Add'}
                            </Button>
                        </Space>
                    </Form.Item>
                </Form>
            </Modal>
        </Layout>
    );
}

export default function App() {
    return (
        <Router>
            <ContactsManager />
        </Router>
    );
}
