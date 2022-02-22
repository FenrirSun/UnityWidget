using System;
using System.Collections.Generic;

/// <summary>
/// 数据模块发现机制
/// 若新加模块，只需扩展 BaseDiscover
/// @data: 2019-01-16
/// @author: LLL
/// </summary>
public static class DiscoverTools
{
    /// <summary>
    /// discover dic
    /// </summary>
    private static Dictionary<Type, BaseDiscover> discoverDic = new Dictionary<Type, BaseDiscover>();

    /// <summary>
    /// 处理函数
    /// </summary>
    private static Dictionary<Type, Action<Type, Attribute>> processDic = new Dictionary<Type, Action<Type, Attribute>>();

    /// <summary>
    /// 发现模块
    /// </summary>
    public static void Discover()
    {
        var types = DiscoverCurrentAssemblyTypes();

        for (var item = types.GetEnumerator(); item.MoveNext();)
        {
            var type = item.Current as Type;
            if (type == null)
                throw new Exception("DiscoverAll by 'Type' is null! ");

            if (type.IsSubclassOf(typeof(BaseDiscover)))
            {
                var typeAttributes = type.GetCustomAttributes(false);
                if (typeAttributes.Length > 0)
                {
                    DiscoverAttribute discover = typeAttributes[0] as DiscoverAttribute;
                    if (!discoverDic.ContainsKey(discover.attributeType))
                    {
                        var discoverInstance = Activator.CreateInstance(type) as BaseDiscover;

                        discoverDic[discover.attributeType] = discoverInstance;

                        processDic[discover.attributeType] = discoverInstance.OnDiscoverModule;
                    }
                }
            }
        }

        for(var item = types.GetEnumerator();item.MoveNext();)
        {
            var type = item.Current as Type;
            if (type == null)
                throw new Exception("DiscoverAll by 'Type' is null! ");

            for(var discover = discoverDic.GetEnumerator();discover.MoveNext();)
            {
                var typeAttribute = type.GetCustomAttributes(discover.Current.Key, false);
                //找到了标记
                if(typeAttribute.Length > 0)
                {
                    discover.Current.Value.OnDiscoverModule(type, typeAttribute[0] as Attribute);
                }
            }
        }

        Debugger.Log("Discover ", $"Module: { ModuleDiscover.DiscoverTypes.Count } " +
            $"Template: { TemplateDiscover.DiscoverTypes.Count } " +
            $"Table: { TableDiscover.DiscoverTypes.Count } " +
            $"UIModule: { UIModuleDiscover.DiscoverTypes.Count }");
    }

    #region Assembly 类型相关

    /// <summary>
    /// 当前Assembly的全部类型
    /// </summary>
    /// <returns></returns>
    public static Type[] DiscoverCurrentAssemblyTypes() => typeof(DiscoverTools).Assembly.GetTypes();
    
    #endregion

    /// <summary>
    /// 清理
    /// </summary>
    public static void Clear()
    {
        for(var item = discoverDic.GetEnumerator();item.MoveNext();)
        {
            item.Current.Value.OnClear();
        }

        discoverDic.Clear();
    }
}
