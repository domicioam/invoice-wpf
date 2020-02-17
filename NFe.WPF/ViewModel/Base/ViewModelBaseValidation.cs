using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.CompilerServices;
using GalaSoft.MvvmLight;

namespace NFe.WPF.ViewModel.Base
{
    public class ViewModelBaseValidation : ViewModelBase, INotifyDataErrorInfo
    {
        private Dictionary<string, List<string>> _validationErrors = new Dictionary<string, List<string>>();

        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged = delegate { };

        public bool HasErrors => _validationErrors.Count > 0;

        public virtual IEnumerable GetErrors(string propertyName)
        {
            if (_validationErrors.ContainsKey(propertyName))
                return _validationErrors[propertyName];
            else
                return null;
        }

        protected void SetProperty<T>(ref T member, T value,
            [CallerMemberName] string propertyName = null)
        {
            if (Equals(member, value)) return;

            member = value;
            RaisePropertyChanged(propertyName);
            ValidateProperty(propertyName, value);
        }

        private void ValidateProperty<T>(string propertyName, T value)
        {
            if (_validationErrors.ContainsKey(propertyName))
                _validationErrors.Remove(propertyName);

            var validationResults = new List<ValidationResult>();
            var validationContext = new ValidationContext(this, null, null) { MemberName = propertyName };

            if (!Validator.TryValidateProperty(value, validationContext, validationResults))
            {
                _validationErrors.Add(propertyName, new List<string>());

                foreach (var validationResult in validationResults)
                {
                    _validationErrors[propertyName].Add(validationResult.ErrorMessage);
                }
            }

            ErrorsChanged(this, new DataErrorsChangedEventArgs(propertyName));
        }

        public void ValidateModel()
        {
            _validationErrors.Clear();
            var validationResults = new List<ValidationResult>();
            var validationContext = new ValidationContext(this, null, null);

            if (!Validator.TryValidateObject(this, validationContext, validationResults, true))
            {
                foreach (var validationResult in validationResults)
                {
                    string property = validationResult.MemberNames.ElementAt(0);
                    if (_validationErrors.ContainsKey(property))
                    {
                        _validationErrors[property].Add(validationResult.ErrorMessage);
                    }
                    else
                    {
                        _validationErrors.Add(property, new List<string> { validationResult.ErrorMessage });
                    }

                    ErrorsChanged(this, new DataErrorsChangedEventArgs(property));
                }
            }
        }
    }
}
