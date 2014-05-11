using System;
using System.ComponentModel;
using System.Reflection;

namespace ssh_tunnel_agent.Classes {
    // based on http://indepthdev.azurewebsites.net/?p=73
    public abstract class EditableObject<T> : NotifyPropertyChangedBase, IEditableObject {
        private T Cache { get; set; }
        internal bool isEditing;

        public void BeginEdit() {
            if (isEditing)
                return;

            isEditing = true;
            Cache = Activator.CreateInstance<T>();

            foreach (PropertyInfo info in typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance))
                if (info.CanRead && info.CanWrite)
                    info.SetValue(Cache, info.GetValue(this));
        }

        public void EndEdit() {
            if (!isEditing)
                return;

            isEditing = false;
            Cache = default(T);
        }

        public void CancelEdit() {
            if (!isEditing)
                return;

            foreach (PropertyInfo info in typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance))
                if (info.CanRead && info.CanWrite)
                    info.SetValue(this, info.GetValue(Cache));

            EndEdit();
        }
    }
}
