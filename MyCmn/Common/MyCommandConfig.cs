using System.Configuration;

namespace MyCmn
{
    /// <summary>
    /// 插件式命令配置节.
    /// </summary>
    /// <example>
    /// <code>
    /// &lt;configuration&gt;
    ///  &lt;configSections&gt;
    ///    &lt;section name="MyCmd" type="MyCmn.MyCommandConfig,MyCmn"/&gt;
    ///  &lt;/configSections&gt;
    ///  &lt;MyCmd&gt;
    ///    &lt;Commands&gt;
    ///      &lt;Command Name="Svn" Type="MyTool.GetFileFromSvnHandler,MyTool"/&gt;
    ///      &lt;Command Name="NewFile" Type="MyTool.NewFileHandler,MyTool"/&gt;
    ///    &lt;/Commands&gt;
    ///  &lt;/MyCmd&gt;
    ///&lt;/configuration&gt;
    /// </code>
    /// </example>
    public class MyCommandConfig : ConfigurationSection
    {
        [ConfigurationProperty("Commands")]
        public CommandCollection Commands
        {
            get { return this["Commands"] as CommandCollection; }
        }

        #region Nested type: CommandCollection

        public class CommandCollection : ConfigurationElementCollection
        {
            public override ConfigurationElementCollectionType CollectionType
            {
                get { return ConfigurationElementCollectionType.BasicMap; }
            }

            protected override string ElementName
            {
                get { return "Command"; }
            }

            protected override ConfigurationElement CreateNewElement()
            {
                return new CommandElement();
            }

            protected override object GetElementKey(ConfigurationElement element)
            {
                var siteUser = element as CommandElement;
                return siteUser.Name;
            }

            #region Nested type: CommandElement

            public class CommandElement : ConfigurationElement
            {
                /// <summary>
                /// 命令名称,主键
                /// </summary>
                [ConfigurationProperty("Name", IsRequired = true, IsKey = true)]
                public string Name
                {
                    get { return this["Name"] as string; }
                    set { this["Name"] = value; }
                }

                /// <summary>
                /// 加载Command的Type
                /// </summary>
                [ConfigurationProperty("Type", IsRequired = true)]
                public string Type
                {
                    get { return (string) (this["Type"]); }
                    set { this["Type"] = value; }
                }
            }

            #endregion
        }

        #endregion
    }
}