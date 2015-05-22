
namespace MyCmn
{
    //public interface IReadEntity : IModel
    //{        
    //    /// <summary>
    //    /// 取属性值.
    //    /// </summary>
    //    /// <param name="PropertyName"></param>
    //    /// <returns></returns>
    //    object GetPropertyValue(string PropertyName);
    //}

    ///// <summary>
    ///// 如果实体实现此接口, 则在对象化实体体,采用该接口的两个方法对对象赋值(避免采用反射)
    ///// </summary>
    ///// <remarks>
    ///// 如果某实体继承了 IEntity，那么该实体最好采用 密封类。如果要扩展该类，请使用组合，而不是继承。
    ///// 如果子类继承了 实现IEntity的类 , 子类应该也重新继承 IEntity , 否则取不到子类新增的属性.
    ///// <example>
    ///// <code>
    ///// public class PersonModel : PersonRule.Entity, IEntity
    ///// {
    /////     public string Dept { get; set; }
    /////     public string Role { get; set; }
    /////
    /////     public PersonModel(PersonRule.Entity Entity)
    /////     {
    /////         Entity.CopyTo(this);
    /////     }
    /////
    /////     string[] IEntity.GetProperties()
    /////     {
    /////         return base.GetProperties().AddOne("Dept").AddOne("Role").ToArray();
    /////     }
    /////
    /////     object IEntity.GetPropertyValue(string PropertyName)
    /////     {
    /////         if (PropertyName == "Dept") return this.Dept;
    /////         else if (PropertyName == "Role") return this.Role;
    /////         else return base.GetPropertyValue(PropertyName);
    /////     }
    /////
    /////     bool IEntity.SetPropertyValue(string PropertyName, object Value)
    /////     {
    /////         if (PropertyName == "Dept")
    /////         {
    /////             this.Dept = Value.AsString();
    /////             return true;
    /////         }
    /////         else if (PropertyName == "Role")
    /////         {
    /////             this.Role = Value.AsString();
    /////             return true;
    /////         }
    /////         else return base.SetPropertyValue(PropertyName, Value);
    /////     }
    ///// }
    ///// </code>
    ///// </example>
    ///// </remarks>
    //public interface IEntity : IReadEntity
    //{
    //    /// <summary>
    //    /// 设置属性值. 
    //    /// </summary>
    //    /// <param name="PropertyName"></param>
    //    /// <param name="Value"></param>
    //    /// <returns>如果找不到该属性, 返回 false.</returns>
    //    bool SetPropertyValue(string PropertyName, object Value);
    //}
}