using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace ContactsManager.Controls
{
    public enum ValidationMode
    {
        Name,
        Phone
    }

    public class ValidationTextBox : TextBox
    {
        public static readonly DependencyProperty ValidationModeProperty =
            DependencyProperty.Register("ValidationMode", typeof(ValidationMode), typeof(ValidationTextBox),
                new PropertyMetadata(ValidationMode.Name));

        public static readonly DependencyProperty ErrorMessageProperty =
            DependencyProperty.Register("ErrorMessage", typeof(string), typeof(ValidationTextBox),
                new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty HasErrorProperty =
            DependencyProperty.Register("HasError", typeof(bool), typeof(ValidationTextBox),
                new PropertyMetadata(false));

        public ValidationMode ValidationMode
        {
            get { return (ValidationMode)GetValue(ValidationModeProperty); }
            set { SetValue(ValidationModeProperty, value); }
        }

        public string ErrorMessage
        {
            get { return (string)GetValue(ErrorMessageProperty); }
            set { SetValue(ErrorMessageProperty, value); }
        }

        public bool HasError
        {
            get { return (bool)GetValue(HasErrorProperty); }
            set { SetValue(HasErrorProperty, value); }
        }

        protected override void OnPreviewTextInput(TextCompositionEventArgs e)
        {
            var current = Text ?? string.Empty;
            var selStart = Math.Min(SelectionStart, current.Length);
            var selLen = Math.Min(SelectionLength, Math.Max(0, current.Length - selStart));
            string newText = current.Remove(selStart, selLen).Insert(selStart, e.Text);

            if (!IsValidInput(newText, e.Text))
            {
                e.Handled = true;
                ShowValidationError(GetValidationErrorMessage(e.Text));
            }
            else
            {
                ClearValidationError();
            }

            base.OnPreviewTextInput(e);
        }

        protected override void OnTextChanged(TextChangedEventArgs e)
        {
            base.OnTextChanged(e);
            ValidateText();
        }

        private bool IsValidInput(string fullText, string inputText)
        {
            switch (ValidationMode)
            {
                case ValidationMode.Name:
                    // Allow letters, apostrophes, hyphens, dots, and spaces
                    return Regex.IsMatch(inputText, @"^[a-zA-Z.'\- ]*$");

                case ValidationMode.Phone:
                    // Allow only digits, and a single leading '+' at the very beginning
                    if (string.IsNullOrEmpty(fullText))
                        return Regex.IsMatch(inputText, @"^[\+0-9]*$");

                    // If input is '+', only allow if caret is at 0 and '+' not already present
                    if (inputText == "+")
                        return !fullText.Contains("+") && SelectionStart == 0;

                    // Otherwise allow only digits
                    return Regex.IsMatch(inputText, @"^[0-9]*$");

                default:
                    return true;
            }
        }

        private void ValidateText()
        {
            if (string.IsNullOrWhiteSpace(Text))
            {
                ShowValidationError($"{GetFieldName()} is required.");
                return;
            }

            switch (ValidationMode)
            {
                case ValidationMode.Name:
                    if (!Regex.IsMatch(Text, @"^[a-zA-Z.'\- ]+$"))
                    {
                        ShowValidationError($"{GetFieldName()} contains invalid characters.");
                    }
                    else
                    {
                        ClearValidationError();
                    }
                    break;

                case ValidationMode.Phone:
                    // Model rules: between 10 and 15 digits, and only digits with optional leading '+'
                    var digits = Regex.Replace(Text, @"[^0-9]", string.Empty);
                    if (digits.Length < 10 || digits.Length > 15)
                    {
                        ShowValidationError("Phone number must be between 10 and 15 digits.");
                    }
                    else if (!Regex.IsMatch(Text, @"^\+?[0-9]+$"))
                    {
                        ShowValidationError("Phone number can only contain digits and a leading '+'.");
                    }
                    else
                    {
                        ClearValidationError();
                    }
                    break;
            }
        }

        private string GetValidationErrorMessage(string inputText)
        {
            switch (ValidationMode)
            {
                case ValidationMode.Name:
                    return $"'{inputText}' is not allowed. Only letters, spaces, apostrophes, hyphens, and dots are permitted.";

                case ValidationMode.Phone:
                    return $"'{inputText}' is not allowed. Only digits and a single leading '+' are permitted.";

                default:
                    return "Invalid input.";
            }
        }

        private string GetFieldName()
        {
            switch (ValidationMode)
            {
                case ValidationMode.Name:
                    return "Name";
                case ValidationMode.Phone:
                    return "Phone number";
                default:
                    return "Field";
            }
        }

        private void ShowValidationError(string message)
        {
            ErrorMessage = message;
            HasError = true;
        }

        private void ClearValidationError()
        {
            ErrorMessage = string.Empty;
            HasError = false;
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            // Allow control keys (backspace, delete, arrow keys, etc.)
            if (e.Key == Key.Back || e.Key == Key.Delete ||
                e.Key == Key.Left || e.Key == Key.Right ||
                e.Key == Key.Home || e.Key == Key.End ||
                e.Key == Key.Tab || e.Key == Key.Enter ||
                (e.Key >= Key.F1 && e.Key <= Key.F24) ||
                (Keyboard.Modifiers & ModifierKeys.Control) != 0)
            {
                base.OnKeyDown(e);
                return;
            }

            base.OnKeyDown(e);
        }

        // Handle paste operations
        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            if (e.Key == Key.V && (Keyboard.Modifiers & ModifierKeys.Control) != 0)
            {
                // Handle paste
                string clipboardText = Clipboard.GetText();
                if (!string.IsNullOrEmpty(clipboardText))
                {
                    var current = Text ?? string.Empty;
                    var selStart = Math.Min(SelectionStart, current.Length);
                    var selLen = Math.Min(SelectionLength, Math.Max(0, current.Length - selStart));
                    string newText = current.Remove(selStart, selLen).Insert(selStart, clipboardText);
                    if (!IsValidPasteText(newText))
                    {
                        e.Handled = true;
                        ShowValidationError($"Pasted text contains invalid characters for {GetFieldName().ToLower()}.");
                    }
                }
            }

            base.OnPreviewKeyDown(e);
        }

        private bool IsValidPasteText(string text)
        {
            switch (ValidationMode)
            {
                case ValidationMode.Name:
                    return Regex.IsMatch(text, @"^[a-zA-Z.'\- ]*$");

                case ValidationMode.Phone:
                    return Regex.IsMatch(text, @"^\+?[0-9]*$");

                default:
                    return true;
            }
        }
    }
}
