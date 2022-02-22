using System;
using System.Collections.Generic;
using ZFrame;

/// <summary>
/// 模板发现
/// @data: 2019-01-16
/// @author: LLL
/// </summary>
[Discover(typeof(TemplateAttribute))]
public class TemplateDiscover : BaseDiscover
{
    /// <summary>
    /// 发现的数据
    /// </summary>
    public static SortedList<TemplateAttribute, Type> DiscoverTypes { get; } = new SortedList<TemplateAttribute, Type>(new AttributeCompare<TemplateAttribute>());

    public override void OnDiscoverModule(Type _type, Attribute _attribute)
    {
        if (_type.IsSubclassOf(typeof(BaseTemplate)))
        {
            var tempAttribute = _attribute as TemplateAttribute;

            if (!DiscoverTypes.ContainsKey(tempAttribute))
            {
                DiscoverTypes.Add(tempAttribute, _type);
            }
            else
            {
                throw new Exception($"TemplateAttribute key is repeat! TemplateName : { tempAttribute.templateName } type : { _type.FullName }");
            }
        }
        else
        {
            throw new Exception($"Not Allow a template not extend Template! { _type.FullName }");
        }
    }

    public override void OnClear()
    {
        DiscoverTypes.Clear();
    }
}
