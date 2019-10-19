using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class BuildReferences {

    public static Dictionary<string, string> externalReferences = new Dictionary<string, string>();
    public static Dictionary<string, string> customScripts = new Dictionary<string, string>();

    public static JArray Scan(GameObject prefab) {
        JArray jComponents = new JArray();
        JObject jAssetsAndScripts = new JObject();
        JArray jScripts = new JArray();
        JArray jAssets = new JArray();
        externalReferences = new Dictionary<string, string>();
        customScripts = new Dictionary<string, string>();
        jAssetsAndScripts["scripts"] = jScripts;
        jAssetsAndScripts["assets"] = jAssets;
        var children = prefab.transform.root.GetComponentsInChildren<Transform>();
        GetAllChildComponents(children, (component) => {
            if (component == null) {
                return;
            }
            JObject jComponent = new JObject();
            jComponent["InstanceID"] = new JValue(component.GetInstanceID());
            jComponent["Type"] = new JValue(component.GetType().ToString());
            jComponent["BaseType"] = new JValue(component.GetType().BaseType.ToString());
            if (component.GetType().BaseType.ToString() == "UnityEngine.MonoBehaviour") {
                AddIfNotAlready(jScripts, new JValue(AssetDatabase.GetAssetPath(MonoScript.FromMonoBehaviour(component))));
            }
            jComponent["path"] = new JValue(GetGameObjectPath(component.transform));
            JArray jFields = new JArray();
            jComponent["Fields"] = jFields;
            if (component.GetType().BaseType.ToString() != "AppBehaviour") {
                jComponents.Add(jComponent);
            }
            if (!component.GetType().ToString().StartsWith("UnityEngine.")) {
                foreach (var field in component.GetType().GetFields()) {
                    try {
                        JObject jField = new JObject();
                        jField["Name"] = new JValue(field.Name);
                        jField["FieldType"] = new JValue(field.FieldType.ToString());
                        jFields.Add(jField);
                        var value = field.GetValue(component);
                        if (value != null && !value.Equals(null)) {
                            switch (field.FieldType.BaseType.ToString()) {
                                case "UnityEngine.MonoBehaviour":
                                    jField["path"] = new JValue(GetGameObjectPath(((MonoBehaviour)value).transform));
                                    AddIfNotAlready(jScripts, new JValue(AssetDatabase.GetAssetPath(MonoScript.FromMonoBehaviour((MonoBehaviour)value))));
                                    jField["InstanceID"] = new JValue(((MonoBehaviour)value).GetInstanceID());
                                    break;
                                case "UnityEngine.Transform":
                                    jField["path"] = new JValue(GetGameObjectPath((Transform)value));
                                    jField["InstanceID"] = new JValue(((Transform)value).GetInstanceID());
                                    break;
                                default:
                                    switch (field.FieldType.ToString()) {
                                        case "UnityEngine.Material":
                                            jField["InstanceID"] = new JValue(((Material)value).GetInstanceID());
                                            jField["path"] = new JValue(AssetDatabase.GetAssetPath((Material)value));
                                            CheckForExternal(jComponent, jField);
                                            AddIfNotAlready(jAssets, new JValue(AssetDatabase.GetAssetPath((Material)value)));
                                            break;
                                        case "System.Boolean":
                                        case "System.String":
                                        case "System.Double":
                                        case "System.Single":
                                        case "System.Int32":
                                            jField["value"] = new JValue(value);
                                            break;
                                        case "UnityEngine.Vector4":
                                            Vector4 vec4 = (Vector4)value;
                                            jField["value"] = JObject.Parse("{\"x\":"+ vec4.x + ",\"y\":" + vec4.y + ",\"z\":" + vec4.z + ",\"w\":" + vec4.w + "}");
                                            break;
                                        case "UnityEngine.Vector3":
                                            Vector3 vec3 = (Vector3)value;
                                            jField["value"] = JObject.Parse("{\"x\":" + vec3.x + ",\"y\":" + vec3.y + ",\"z\":" + vec3.z + "}");
                                            break;
                                        case "UnityEngine.Vector2":
                                            Vector2 vec2 = (Vector2)value;
                                            jField["value"] = JObject.Parse("{\"x\":" + vec2.x + ",\"y\":" + vec2.y + "}");
                                            break;
                                        case "UnityEngine.Quaternion":
                                            Quaternion quat = (Quaternion)value;
                                            jField["value"] = JObject.Parse("{\"x\":" + quat.x + ",\"y\":" + quat.y + ",\"z\":" + quat.z + ",\"w\":" + quat.w + "}");
                                            break;
                                        default:
                                            jField["path"] = new JValue(GetGameObjectPath(((Component)value).transform));
                                            jField["InstanceID"] = new JValue(((Component)value).GetInstanceID());
                                            CheckForExternal(jComponent, jField);
                                            break;
                                    }
                                    break;
                            }
                        }
                    } catch (Exception e) {
                        Debug.LogError(e);
                    }
                }
            }
        });
        //Debug.Log(jComponents);
        //Debug.Log(jAssetsAndScripts);
        return jComponents;
    }
    static void CheckForExternal(JObject jComponent, JObject jField) {
        if (!((string)jField["path"]).StartsWith((string)jComponent["path"])) {
            externalReferences.Add((string)jField["InstanceID"], (string)jField["Name"] + " [" + jField["FieldType"] + "]: " + jField["path"]);
        }
    }

    static void AddIfNotAlready(JArray values, JValue value) {
        if (!values.ToObject<List<string>>().Contains((string)value)) {
            values.Add(value);
        }
    }

    static void GetAllChildComponents(Transform[] children, Action<MonoBehaviour> callback) {
        foreach (var child in children) {
            MonoBehaviour[] components = child.GetComponents<MonoBehaviour>();
            for (int i = 0; i < components.Length; i++) {
                callback(components[i]);
            }
        }
    }

    public static string GetGameObjectPath(Transform transform) {
        string path = transform.name;
        while (transform.parent != null) {
            transform = transform.parent;
            path = transform.name + "/" + path;
        }
        return path;
    }
}
