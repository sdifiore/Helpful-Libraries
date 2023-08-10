﻿using Lombiq.HelpfulLibraries.OrchardCore.Contents;
using Microsoft.AspNetCore.Mvc.Localization;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Helpers;

namespace Lombiq.HelpfulLibraries.OrchardCore.Workflow;

public abstract class SimpleEventActivityDisplayDriver<TActivity> : DisplayDriver<IActivity, TActivity>
    where TActivity : SimpleEventActivity
{
    public abstract string IconClass { get; }
    public abstract LocalizedHtmlString Title { get; }
    public abstract LocalizedHtmlString Description { get; }

    private string IconHtml => string.IsNullOrEmpty(IconClass) ? string.Empty : $"<i class=\"fa {IconClass}\"></i>";

    public override IDisplayResult Display(TActivity model) =>
        Combine(
            this.RawHtml(ThumbnailHtml()).Location(CommonContentDisplayTypes.Thumbnail, CommonLocationNames.Content),
            this.RawHtml(DesignHtml(model)).Location(CommonContentDisplayTypes.Design, CommonLocationNames.Content));

    private string ThumbnailHtml() =>
        $"<h4 class=\"card-title\">{IconHtml}{Title}</h4><p>{Description}</p>";

    private string DesignHtml(TActivity model)
    {
        var title = model.GetTitleOrDefault(() => Title);
        return $"<header><h4>{IconHtml}{title}</h4></header>";
    }
}
