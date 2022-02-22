using System;
using System.Collections.Generic;
using ZFrame;

/// <summary>
/// module 发现
/// @data: 2019-01-16
/// @author: LLL
/// </summary>
[Discover(typeof(ModuleAttribute))]
public class ModuleDiscover : BaseDiscover
{
    /// <summary>
    /// 发现的数据
    /// </summary>
    public static SortedDictionary<ModuleAttribute, Type> DiscoverTypes { get; } = new SortedDictionary<ModuleAttribute, Type>(new AttributeCompare<ModuleAttribute>());

    public override void OnDiscoverModule(Type _type, Attribute _attribute)
    {
        if (_type.IsSubclassOf(typeof(Module)))
        {
            var moduleAttribute = _attribute as ModuleAttribute;

            if (!DiscoverTypes.ContainsKey(moduleAttribute))
            {
                DiscoverTypes.Add(_attribute as ModuleAttribute, _type);

                RegisterModule(_type);
            }
            else
            {
                throw new Exception($"ModuleAttribute key is repeat! TemplateName : { moduleAttribute.moduleName } type : { _type.FullName }");
            }
        }
        else
        {
            throw new Exception($"Not Allow a module not extend Module! { _type.FullName }");
        }
    }

    /// <summary>
    /// 注册Module
    /// </summary>
    /// <param name="_type"></param>
    private void RegisterModule(Type _type)
    {
        Module moduleInstance = (Module)Activator.CreateInstance(_type);
        Frame.instance.RegisterModule(moduleInstance);
    }

    public override void OnClear()
    {
        DiscoverTypes.Clear();
    }
}
