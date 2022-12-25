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
}