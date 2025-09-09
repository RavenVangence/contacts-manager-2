import { useEffect, useState } from 'react';
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
    Select, 
    Typography,
    Divider,
    Popconfirm
} from 'antd';
import { PlusOutlined, EditOutlined, DeleteOutlined, SearchOutlined } from '@ant-design/icons';

const { Title } = Typography;
const { Search } = Input;
const { Option } = Select;

export default function App() {
    const [contacts, setContacts] = useState([]);
    const [filteredContacts, setFilteredContacts] = useState([]);
    const [loading, setLoading] = useState(false);
    const [modalVisible, setModalVisible] = useState(false);
    const [editingContact, setEditingContact] = useState(null);
    const [searchTerm, setSearchTerm] = useState('');
    const [filterUsed, setFilterUsed] = useState('all');
    const [form] = Form.useForm();

    const load = async () => {
        setLoading(true);
        try {
            const data = await ContactsApi.list();
            setContacts(data);
            filterContacts(data, searchTerm, filterUsed);
        } catch (error) {
            message.error('Failed to load contacts: ' + error.message);
        } finally {
            setLoading(false);
        }
    };

    useEffect(() => {
        load();
    }, []);

    const filterContacts = (contactsList, search, filter) => {
        let filtered = contactsList.filter(c => {
            const matchesSearch = search === '' || 
                c.firstName.toLowerCase().includes(search.toLowerCase()) ||
                c.lastName.toLowerCase().includes(search.toLowerCase()) ||
                c.phone.includes(search);
            
            const matchesFilter = filter === 'all' || 
                (filter === 'used' && c.used) ||
                (filter === 'unused' && !c.used);
            
            return matchesSearch && matchesFilter;
        });
        
        setFilteredContacts(filtered);
    };

    useEffect(() => {
        filterContacts(contacts, searchTerm, filterUsed);
    }, [contacts, searchTerm, filterUsed]);

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
            load();
        } catch (error) {
            message.error('Failed to update contact: ' + error.message);
        }
    };

    const columns = [
        {
            title: 'First Name',
            dataIndex: 'firstName',
            key: 'firstName',
            sorter: (a, b) => a.firstName.localeCompare(b.firstName),
        },
        {
            title: 'Last Name',
            dataIndex: 'lastName',
            key: 'lastName',
            sorter: (a, b) => a.lastName.localeCompare(b.lastName),
        },
        {
            title: 'Phone',
            dataIndex: 'phone',
            key: 'phone',
            sorter: (a, b) => a.phone.localeCompare(b.phone),
        },
        {
            title: 'Used',
            dataIndex: 'used',
            key: 'used',
            sorter: (a, b) => (a.used ? 1 : 0) - (b.used ? 1 : 0),
            render: (used, record) => (
                <Checkbox 
                    checked={used} 
                    onChange={() => toggleUsed(record)}
                />
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

    return (
        <div style={{ padding: '24px', maxWidth: '1200px', margin: '0 auto' }}>
            <Card>
                <Row justify="space-between" align="middle" style={{ marginBottom: '24px' }}>
                    <Col>
                        <Title level={2} style={{ margin: 0 }}>
                            Contacts Manager
                        </Title>
                    </Col>
                    <Col>
                        <Button
                            type="primary"
                            icon={<PlusOutlined />}
                            onClick={handleAdd}
                            size="large"
                        >
                            Add Contact
                        </Button>
                    </Col>
                </Row>

                <Divider />

                <Row gutter={16} style={{ marginBottom: '24px' }}>
                    <Col xs={24} sm={12} md={8}>
                        <Search
                            placeholder="Search by name or phone..."
                            allowClear
                            prefix={<SearchOutlined />}
                            value={searchTerm}
                            onChange={(e) => setSearchTerm(e.target.value)}
                            size="large"
                        />
                    </Col>
                    <Col xs={24} sm={12} md={8}>
                        <Select
                            value={filterUsed}
                            onChange={setFilterUsed}
                            style={{ width: '100%' }}
                            size="large"
                        >
                            <Option value="all">All Contacts</Option>
                            <Option value="used">Used Only</Option>
                            <Option value="unused">Unused Only</Option>
                        </Select>
                    </Col>
                    <Col xs={24} sm={24} md={8} style={{ textAlign: 'right' }}>
                        <div style={{ lineHeight: '40px', color: '#666' }}>
                            {filteredContacts.length} of {contacts.length} contacts
                        </div>
                    </Col>
                </Row>

                <Table
                    columns={columns}
                    dataSource={filteredContacts}
                    rowKey="id"
                    loading={loading}
                    pagination={{
                        pageSize: 10,
                        showSizeChanger: true,
                        showQuickJumper: true,
                        showTotal: (total, range) => 
                            `${range[0]}-${range[1]} of ${total} contacts`,
                    }}
                />
            </Card>

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
                        label="First Name"
                        rules={[{ required: true, message: 'Please enter first name!' }]}
                    >
                        <Input placeholder="Enter first name" />
                    </Form.Item>

                    <Form.Item
                        name="lastName"
                        label="Last Name"
                        rules={[{ required: true, message: 'Please enter last name!' }]}
                    >
                        <Input placeholder="Enter last name" />
                    </Form.Item>

                    <Form.Item
                        name="phone"
                        label="Phone"
                        rules={[{ required: true, message: 'Please enter phone number!' }]}
                    >
                        <Input placeholder="Enter phone number" />
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
        </div>
    );
}
