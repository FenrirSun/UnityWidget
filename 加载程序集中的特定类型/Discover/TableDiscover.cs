using System;
using System.Collections.Generic;
using ZLib.Data;

/// <summary>
/// 表发现
/// @data: 2019-01-16
/// @author: LLL
/// </summary>
[Discover(typeof(TableNameAttribute))]
public class TableDiscover : BaseDiscover
{
    /// <summary>
    /// 发现的数据
    /// </summary>
    public static SortedList<TableNameAttribute, Type> DiscoverTypes { get; } = new SortedList<TableNameAttribute, Type>(new AttributeCompare<TableNameAttribute>());

    public override void OnDiscoverModule(Type _type, Attribute _attribute)
    {
        if (_type.IsSubclassOf(typeof(TableContent)))
        {
            var tbAttribute = _attribute as TableNameAttribute;

            if (!DiscoverTypes.ContainsKey(tbAttribute))
            {
                DiscoverTypes.Add(tbAttribute, _type);
            }
            else
            {
                throw new Exception($"TableNameAttribute key is repeat! TableName : { tbAttribute.tableName } type : { _type.FullName }");
            }
        }
        else
        {
            throw new Exception($"Not Allow a data not extend TableContent! { _type.FullName }");
        }
    }

    public override void OnClear()
    {
        DiscoverTypes.Clear();
    }
}
