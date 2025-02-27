using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using SETHD.Utilis;
using System.Reflection;
using System.Collections.Generic;
using Object = UnityEngine.Object;

namespace SETHD.Utils.Editor
{
    [CustomPropertyDrawer(typeof(InterfaceReference<>))]
    [CustomPropertyDrawer(typeof(InterfaceReference<,>))]
    public class InterfaceReferenceDrawer : PropertyDrawer
    {
        private const string UNDERLYING_VALUE_FIELD_NAME = "underlyingValue";
    
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            var underlyingProperty = property.FindPropertyRelative(UNDERLYING_VALUE_FIELD_NAME);
            var args = GetArguments(fieldInfo);
    
            EditorGUI.BeginProperty(position, label, property);
    
            var assignedObject = EditorGUI.ObjectField(position, label, underlyingProperty.objectReferenceValue, args.ObjectType, true);
    
            if (assignedObject != null) {
                Object component = null;
    
                if (assignedObject is GameObject gameObject) {
                    component = gameObject.GetComponent(args.InterfaceType);
                } else if (args.InterfaceType.IsAssignableFrom(assignedObject.GetType())) {
                    component = assignedObject;
                }
    
                if (component != null) {
                    ValidateAndAssignObject(underlyingProperty, component, component.name, args.InterfaceType.Name);
                } else {
                    Debug.LogWarning($"Assigned object does not implement required interface '{args.InterfaceType.Name}'.");
                    underlyingProperty.objectReferenceValue = null;
                }
            } else {
                underlyingProperty.objectReferenceValue = null;
            }
    
    
            EditorGUI.EndProperty();
            InterfaceReferenceUtil.OnGUI(position, underlyingProperty, label, args);
        }
    
        private static InterfaceArgs GetArguments(FieldInfo fieldInfo) {
            Type objectType = null, interfaceType = null;
            Type fieldType = fieldInfo.FieldType;
    
            if (!TryGetTypesFromInterfaceReference(fieldType, out objectType, out interfaceType)) {
                GetTypesFromList(fieldType, out objectType, out interfaceType);
            }
            
            return new InterfaceArgs(objectType, interfaceType);
        }
        
        private static bool TryGetTypesFromInterfaceReference(Type type, out Type objType, out Type intfType) {
            objType = intfType = null;
                
            if (type?.IsGenericType != true) return false;
                
            var genericType = type.GetGenericTypeDefinition();
            if (genericType == typeof(InterfaceReference<>)) type = type.BaseType;
    
            if (type?.GetGenericTypeDefinition() == typeof(InterfaceReference<,>)) {
                var types = type.GetGenericArguments();
                intfType = types[0];
                objType = types[1];
                return true;
            }
                
            return false;
        }
        
        private static void GetTypesFromList(Type type, out Type objType, out Type intfType){
            objType = intfType = null;
                
            var listInterface = type.GetInterfaces()
                .FirstOrDefault(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IList<>));
    
            if (listInterface != null) {
                var elementType = listInterface.GetGenericArguments()[0];
                TryGetTypesFromInterfaceReference(elementType, out objType, out intfType);
            }
        }
    
        private static void ValidateAndAssignObject(SerializedProperty property, Object targetObject, string componentNameOrType, string interfaceName = null) {
            if (targetObject != null) {
                property.objectReferenceValue = targetObject;
            } else {
                var message = interfaceName != null
                    ? $"GameObject '{componentNameOrType}'"
                    : "assigned object";
    
                Debug.LogWarning(
                    $"The {message} does not have a component that implements '{interfaceName}'."
                );
                property.objectReferenceValue = null;
            }
        }
    }

    public struct InterfaceArgs 
    {
        public readonly Type ObjectType;
        public readonly Type InterfaceType;
    
        public InterfaceArgs(Type objectType, Type interfaceType) {
            Debug.Assert(typeof(Object).IsAssignableFrom(objectType), $"{nameof(objectType)} needs to be of Type {typeof(Object)}.");
            Debug.Assert(interfaceType.IsInterface, $"{nameof(interfaceType)} needs to be an interface.");
            
            ObjectType = objectType;
            InterfaceType = interfaceType;
        }
    }
}