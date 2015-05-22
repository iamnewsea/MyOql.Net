using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Collections;
using System.Reflection;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace MyCmn
{
    public static partial class ValueProc
    {
        public static event Func<object, PropertyInfo, object> OnGetPropertyValue = null;
        public static event Action<object, PropertyInfo, object> OnSetPropertyValue = null;

        public static event Func<object, FieldInfo, object> OnGetFieldValue = null;
        public static event Action<object, FieldInfo, object> OnSetFieldValue = null;


        public static event Func<object, MethodInfo, object[], object> OnInvoke = null;

        public static object GetMyValue(this PropertyInfo property, object obj)
        {
            if (property == null) return null;

            if (property.CanRead == false) return null;

            if (OnGetPropertyValue == null)
            {
                return property.GetValue(obj, null);
            }

            return OnGetPropertyValue(obj, property);
        }

        public static void SetMyValue(this PropertyInfo property, object obj, object value)
        {
            if (property == null) return;

            if (property.CanWrite == false) return;

            if (OnSetPropertyValue == null)
            {
                property.SetValue(obj, value, null);
                return;
            }


            OnSetPropertyValue(obj, property, ValueProc.AsType(property.PropertyType, value));
        }


        public static object GetMyValue(this FieldInfo property, object obj)
        {
            if (object.Equals(obj, null)) return null;
            if (property == null) return null;

            if (OnGetFieldValue == null)
            {
                return property.GetValue(obj);
            }

            return OnGetFieldValue(obj, property);
        }

        public static void SetMyValue(this FieldInfo property, object obj, object value)
        {
            if (object.Equals(obj, null)) return;
            if (property == null) return;

            if (OnSetFieldValue == null)
            {
                property.SetValue(obj, value);
                return;
            }

            OnSetFieldValue(obj, property, ValueProc.AsType(property.FieldType, value));
        }

        public static object Invoke(this MethodInfo method, object obj, object[] arguments)
        {
            if (object.Equals(obj, null)) return null;
            if (method == null) return null;

            if (OnInvoke == null)
            {
                return method.Invoke(obj, arguments);
            }

            return OnInvoke(obj, method, arguments);
        }


        /// <summary>
        /// MemoryStream 方式深克隆,要求对象标记为 Serializable 特性
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="Entity"></param>
        /// <returns></returns>
        public static T CloneEntity<T>(this T Entity)
        {
            if (Entity == null) return default(T);

            var type = typeof(T);
            if (type.IsPrimitive) return Entity;
            if (type.IsValueType) return Entity;

            using (MemoryStream stream = new MemoryStream())
            {
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Context = new System.Runtime.Serialization.StreamingContext(System.Runtime.Serialization.StreamingContextStates.Clone);
                formatter.Serialize(stream, Entity);
                stream.Position = 0;
                return (T)formatter.Deserialize(stream);
            }
        }

        /// <summary>
        /// 使用IEntity 提供的方法进行对象属性拷贝.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        /// <param name="toEntity"></param>
        /// <returns></returns>
        public static void CopyTo<T>(this object entity, T toEntity)
            where T : class
        {
            GodError.Check(entity == null, "拷贝源不能为空");
            GodError.Check(toEntity == null, "拷贝目标对象不能为空");

            var entityType = entity.GetType();
            var toType = typeof(T);

            {
                var ps1 = entityType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
                var ps2 = toType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
                ps1.Intersect(ps2, new CommonEqualityComparer<PropertyInfo>((a, b) => a.Name == b.Name && a.PropertyType == b.PropertyType))
                .All(p =>
                    {
                        var val = ValueProc.GetMyValue(ps1.First(o => o.Name == p.Name), entity);
                        ValueProc.SetMyValue(ps2.First(o => o.Name == p.Name), toEntity, val);
                        return true;
                    });
            }
            {
                var fs1 = entityType.GetFields(BindingFlags.Public | BindingFlags.Instance);
                var fs2 = toType.GetFields(BindingFlags.Public | BindingFlags.Instance);

                fs1.Intersect(fs2, new CommonEqualityComparer<FieldInfo>((a, b) => a.Name == b.Name && a.FieldType == b.FieldType))
                .All(p =>
                {
                    var val = ValueProc.GetMyValue(fs1.First(o => o.Name == p.Name), entity);
                    ValueProc.SetMyValue(fs2.First(o => o.Name == p.Name), toEntity, val);
                    return true;
                });
            }
        }

        /// <summary>
        /// 把 Model 转为 字典，是一个和  ModelToDictionary(RuleBase Entity, IModel objModel) 相同算法的函数。
        /// </summary>
        /// <param name="objModel"></param>
        /// <returns></returns>
        public static StringDict Model2StringDict(object objModel)
        {
            if (objModel == null) return null;

            StringDict dictModel = objModel as StringDict;
            if (dictModel != null)
            {
                return dictModel;
            }


            IDictionary dict = objModel as IDictionary;
            if (dict != null)
            {
                dictModel = new StringDict();

                foreach (var strKey in dict.Keys)
                {
                    dictModel.Add(strKey.AsString(), dict[strKey].AsString(null));
                }
                return dictModel;
            }

            dictModel = new StringDict();


            Type type = objModel.GetType();

            GodError.Check(type.IsPrimitive, "GodError", "基元类型不能做为 Model", objModel.ToString());

            foreach (PropertyInfo prop in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                dictModel.Add(prop.Name, GetMyValue(prop, objModel).AsString(null));
            }


            foreach (FieldInfo prop in type.GetFields(BindingFlags.Public | BindingFlags.Instance))
            {
                dictModel.Add(prop.Name, GetMyValue(prop, objModel).AsString(null));
            }

            return dictModel;
        }


        /// <summary>
        /// 把字典解析到 Model 类型的 Model 上。
        /// <remarks>
        /// 从数据库返回数据实体时使用,解析如下类型： 
        /// String
        /// IDictionary
        /// 类(支持递归赋值。如果第一级属性找不到，则查找第二级非基元属性，依次向下查找。)
        /// Json树格式，如果在HTML中Post Json对象，如 cols[id][sid] = 10 则可以映射到合适的对象上。
        /// 值类型结构体,主要适用于 数值，Enum类型。对于结构体，把 结果集第一项值 强制类型转换为该结构体类型，所以尽量避免使用自定义结构体。
        /// </remarks>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="Dict"></param>
        /// <param name="NewModelFunc">关键是 泛型！Model可以为null</param>
        /// <returns></returns>
        public static T StringDict2Model<T>(this StringDict Dict, T Model)
        {
            if (Dict == null)
            {
                return default(T);
            }


            T ret = Model;
            Type type = ret.GetType();
            var typeCode = Type.GetTypeCode(type);

            //Object 表示没有反射出 T 类型，  IsInterface 时亦然。
            //if (type.FullName == "System.Object" || type.IsInterface)
            //{
            //}

            //当是结构体时，只返回第一个列。如 Int。
            if (type.IsValueType && type.IsSimpleType())
            {
                return ValueProc.As<T>(Dict.ElementAt(0).Value);
            }

            if (typeCode == TypeCode.String || type.FullName == "System.Text.StringBuilder" || type.FullName == "MyCmn.StringLinker")
            {
                return ValueProc.As<T>(Dict.ElementAt(0).Value);
            }

            else if (type.IsClass)
            {
                //if (object.Equals(ret, default(T)))
                //{
                //    ret = NewModelFunc();
                //}

                var retDict = ret as IDictionary;

                if (retDict != null)
                {
                    var genericTypes = type.getGenericType().GetGenericArguments();
                    if (genericTypes.Length == 2)
                    {
                        foreach (var kv in Dict)
                        {
                            retDict[ValueProc.AsType(genericTypes[0], kv.Key)] = ValueProc.AsType(genericTypes[1], kv.Value);
                        }
                    }
                    else
                    {
                        foreach (var kv in Dict)
                        {
                            retDict[kv.Key] = kv.Value;
                        }
                    }
                    return (T)(object)retDict;
                }


                Dict2DeepObj(Dict, ret);

                return ret;
            }

            throw new GodError("不支持的Model类型！");
        }

        /// <summary>
        /// 可以对子对象赋值。
        /// </summary>
        /// <remarks>
        /// 子对象赋值： Key 如： book.Id 形式，则是给 model 的 book 对象赋 Id 值。
        /// model对象在默认构造函数时，应对 book 子对象初始化。
        /// </remarks>
        /// <typeparam name="T"></typeparam>
        /// <param name="Dict"></param>
        /// <param name="model"></param>
        public static void Dict2DeepObj(IDictionary Dict, object model)
        {
            if (object.Equals(model, null)) return;

            Type TypeOfT = model.GetType();
            Dictionary<string, Dictionary<string, object>> LeftKeys = new Dictionary<string, Dictionary<string, object>>();
            foreach (var key in Dict.Keys)
            {
                if (key.HasValue() == false) continue;

                var val = Dict[key];
                var strKey = key.ToString();

                var index = strKey.IndexOf('.');
                if (index > 0)
                {
                    var k = strKey.Substring(0, index);
                    if (LeftKeys.ContainsKey(k) == false)
                    {
                        LeftKeys[k] = new Dictionary<string, object>();
                    }

                    LeftKeys[k][strKey.Substring(index + 1, strKey.Length - index - 1)] = Dict[key];
                    continue;
                }

                var propInfo = TypeOfT.GetProperty(strKey);
                if (propInfo != null)
                {
                    ValueProc.SetMyValue(propInfo, model, val);
                    continue;
                }

                var fieldInfo = TypeOfT.GetField(strKey);
                if (fieldInfo != null)
                {
                    ValueProc.SetMyValue(fieldInfo, model, val);
                }
            }


            if (LeftKeys.Count > 0)
            {
                foreach (var group in LeftKeys)
                {
                    var prefix = group.Key;
                    var subDict = group.Value;

                    {
                        var pi = TypeOfT.GetProperty(prefix);
                        if (pi != null)
                        {
                            var subValue = ValueProc.GetMyValue(pi, model);
                            if (object.Equals(subValue, null))
                            {
                                subValue = Activator.CreateInstance(pi.PropertyType);
                                ValueProc.SetMyValue(pi, model, subValue);
                            }

                            Dict2DeepObj(subDict, subValue);
                            continue;
                        }
                    }

                    {
                        var fi = TypeOfT.GetField(prefix);
                        if (fi != null)
                        {
                            var subValue = ValueProc.GetMyValue(fi, model);
                            if (object.Equals(subValue, null))
                            {
                                subValue = Activator.CreateInstance(fi.FieldType);
                                ValueProc.SetMyValue(fi, model, subValue);
                            }

                            Dict2DeepObj(subDict, subValue);
                            continue;
                        }
                    }
                }
            }
        }
    }
}