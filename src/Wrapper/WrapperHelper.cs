using System.Linq.Expressions;
using System.Reflection;

namespace ReRender.Wrapper;

public static class WrapperHelper
{
    public static TRet CreateGetter<TObj, TRet>(string field)
    {
        var fieldInfo = typeof(TObj).GetField(field, BindingFlags.NonPublic | BindingFlags.Instance);

        var obj = Expression.Parameter(typeof(TObj), "o");

        var getterFunc = Expression.Lambda<TRet>(
            Expression.MakeMemberAccess(obj, fieldInfo!),
            obj
        );

        return getterFunc.Compile();
    }

    public static TRet CreateSetter<TObj, TParam, TRet>(string field)
    {
        var fieldInfo = typeof(TObj).GetField(field, BindingFlags.NonPublic | BindingFlags.Instance);

        var obj = Expression.Parameter(typeof(TObj), "o");
        var val = Expression.Parameter(typeof(TParam), "p");

        var setterFunc = Expression.Lambda<TRet>(
            Expression.Assign(Expression.MakeMemberAccess(obj, fieldInfo!), val),
            obj, val
        );

        return setterFunc.Compile();
    }
}