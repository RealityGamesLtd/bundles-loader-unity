using System.Collections.Generic;
using System.Reflection;

namespace BundlesLoader.Utils
{
    public static class Utils
    {
        // Gets value from SerializedProperty - even if value is nested
        public static object GetValue(this UnityEditor.SerializedProperty property)
        {
            object obj = property.serializedObject.targetObject;
            foreach (var path in property.propertyPath.Split('.'))
            {
                var type = obj.GetType();
                FieldInfo field = type.GetField(path);
                obj = field.GetValue(obj);
            }
            return obj;
        }

        // Sets value from SerializedProperty - even if value is nested
        public static void SetValue(this UnityEditor.SerializedProperty property, object val)
        {
            object obj = property.serializedObject.targetObject;

            List<KeyValuePair<FieldInfo, object>> list = new List<KeyValuePair<FieldInfo, object>>();
            foreach (var path in property.propertyPath.Split('.'))
            {
                var type = obj.GetType();
                FieldInfo field = type.GetField(path);
                list.Add(new KeyValuePair<FieldInfo, object>(field, obj));
                obj = field.GetValue(obj);
            }

            // Now set values of all objects, from child to parent
            for (int i = list.Count - 1; i >= 0; --i)
            {
                list[i].Key.SetValue(list[i].Value, val);
                // New 'val' object will be parent of current 'val' object
                val = list[i].Value;
            }
        }
    }
}

