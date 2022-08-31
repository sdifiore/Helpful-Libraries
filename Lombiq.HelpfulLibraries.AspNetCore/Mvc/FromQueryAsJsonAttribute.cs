using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Lombiq.HelpfulLibraries.AspNetCore.Mvc;

public sealed class FromQueryAsJsonAttribute : ModelBinderAttribute
{
    public FromQueryAsJsonAttribute()
    {
        BinderType = typeof(JsonModelBinder);
        BindingSource = BindingSource.Query;
    }
}
