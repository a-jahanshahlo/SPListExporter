using FluentValidation;
using FluentValidation.Results;
using SpExport.Core.Contract;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using MahApps.Metro.Controls.Dialogs;
using SpExport.Util.Extention;

namespace SpExport.Core
{
    
    public abstract class ObjectBase : NotificationObject, IDirtyCapable, IExtensibleDataObject, IDataErrorInfo  
    {
        public ObjectBase()
        {
            _Validator = GetValidator();
            Validate();
        }
        private IDialogCoordinator dialogCoordinator;
        protected bool _IsDirty = false;
        protected IValidator _Validator = null;

        protected IEnumerable<ValidationFailure> _ValidationErrors = null;
        // this object play Container role in MEF
        //Can use to instansiate new object
        //  public static IContainer Container { get{return  IocConfig.Container; } }

        #region IExtensibleDataObject Members
        // this is for serialization
        // in example: if we decide to develop second version of app 
        //and in that version added new fields, when client
        //try to get response from server the server never serialize new added fields.
        [IgnoreDataMember]
        public ExtensionDataObject ExtensionData { get; set; }

        #endregion

        #region IDirtyCapable members
        //this custom attribute uses to prevent access in reflection for dirty
        [NotNavigable]
        [IgnoreDataMember]
        public virtual bool IsDirty
        {
            get { return _IsDirty; }
            protected set
            {
                _IsDirty = value;
                OnPropertyChanged("IsDirty", false);
            }
        }

        public virtual bool IsAnythingDirty()
        {
            bool isDirty = false;

            WalkObjectGraph(
                o =>
                {
                    if (o.IsDirty)
                    {
                        isDirty = true;
                        return true; // short circuit
                    }
                    else
                        return false;
                }, coll => { });

            return isDirty;
        }
        // the dirty means uses when an object on the form change state!
        // in real this terminology uses when we want to insure none of fields state are changed.
        public List<IDirtyCapable> GetDirtyObjects()
        {
            List<IDirtyCapable> dirtyObjects = new List<IDirtyCapable>();

            WalkObjectGraph(
                o =>
                {
                    if (o.IsDirty)
                        dirtyObjects.Add(o);

                    return false;
                }, coll => { });

            return dirtyObjects;
        }

        /// <summary>
        /// Walks the object graph cleaning any dirty object.
        /// </summary>
        public void CleanAll()
        {
            WalkObjectGraph(
                o =>
                {
                    if (o.IsDirty)
                        o.IsDirty = false;
                    return false;
                }, coll => { });
        }

        #endregion

        #region Protected methods

        protected void WalkObjectGraph(Func<ObjectBase, bool> snippetForObject,
            Action<IList> snippetForCollection,
            params string[] exemptProperties)
        {
            List<ObjectBase> visited = new List<ObjectBase>();
            Action<ObjectBase> walk = null;

            List<string> exemptions = new List<string>();
            if (exemptProperties != null)
                exemptions = exemptProperties.ToList();

            walk = (o) =>
            {
                if (o != null && !visited.Contains(o))
                {
                    visited.Add(o);

                    bool exitWalk = snippetForObject.Invoke(o);

                    if (!exitWalk)
                    {
                        PropertyInfo[] properties = o.GetBrowsableProperties();
                        foreach (PropertyInfo property in properties)
                        {
                            if (!exemptions.Contains(property.Name))
                            {
                                if (property.PropertyType.IsSubclassOf(typeof(ObjectBase)))
                                {
                                    ObjectBase obj = (ObjectBase)(property.GetValue(o, null));
                                    walk(obj);
                                }
                                else
                                {
                                    IList coll = property.GetValue(o, null) as IList;
                                    if (coll != null)
                                    {
                                        snippetForCollection.Invoke(coll);

                                        foreach (object item in coll)
                                        {
                                            if (item is ObjectBase)
                                                walk((ObjectBase)item);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            };

            walk(this);
        }

        #endregion

        #region Property change notification

        protected override void OnPropertyChanged(string propertyName)
        {
            OnPropertyChanged(propertyName, true);
        }

        protected void OnPropertyChanged<T>(Expression<Func<T>> propertyExpression, bool makeDirty)
        {
            string propertyName = PropertySupport.ExtractPropertyName(propertyExpression);
            OnPropertyChanged(propertyName, makeDirty);
        }

        protected void OnPropertyChanged(string propertyName, bool makeDirty)
        {
            base.OnPropertyChanged(propertyName);

            if (makeDirty)
                IsDirty = true;

            Validate();
        }

        #endregion

        #region Validation

        protected virtual IValidator GetValidator()
        {
            return null;
        }

        [IgnoreDataMember]
        [NotNavigable]
        public IEnumerable<ValidationFailure> ValidationErrors
        {
            get { return _ValidationErrors; }
            set { }
        }

        public void Validate()
        {
            if (_Validator != null)
            {
                ValidationResult results = _Validator.Validate(this);
                _ValidationErrors = results.Errors;
            }
        }
        [IgnoreDataMember]
        [NotNavigable]
        public virtual bool IsValid
        {
            get
            {
                if (_ValidationErrors != null && _ValidationErrors.Count() > 0)
                    return false;
                else
                    return true;
            }
        }

        #endregion

        #region IDataErrorInfo members
        [IgnoreDataMember]
        string IDataErrorInfo.Error
        {
            get { return string.Empty; }
        }
        [IgnoreDataMember]
        string IDataErrorInfo.this[string columnName]
        {
            get
            {
                StringBuilder errors = new StringBuilder();

                if (_ValidationErrors != null && _ValidationErrors.Count() > 0)
                {
                    foreach (ValidationFailure validationError in _ValidationErrors)
                    {
                        if (validationError.PropertyName == columnName)
                            errors.AppendLine(validationError.ErrorMessage);
                    }
                }

                return errors.ToString();
            }
        }

        #endregion

        public virtual bool Prev()
        {
            return true;
        }
        public virtual bool Next()
        {
            return true;
        }

        protected void Serialize(object t)
        {
            Serializer.SerializeToXML (t, t.GetType().FullName); 
        }
        protected T DeSerialize<T>() where T : new()
        {
          
            if (System.IO.File.Exists(typeof(T).FullName))
            {
                var dd = Serializer.DeserializeFromXML<T>(typeof(T).FullName);
                if (dd != null)
                {
                    return dd;
                }
                else
                {
                    return new T();
                }
            }

            return new T();
        }
    }
}
