﻿using OrchardCore.ContentManagement.Metadata.Settings;

namespace OrchardCore.ContentManagement.Metadata.Builders
{
    public static class ContentTypeDefinitionBuilderExtensions
    {
        /// <summary>
        /// Merges all <see langword="bool"/> content type settings; the same ones you have checkboxes for in the admin
        /// dashboard's content type editor in the same order. All values have a default false value, so you can set any
        /// combination using parameter names in a way that signals intentionality (ie. if you explicitly want to deny
        /// an ability or you just don't care) which is useful clarification for later updates in the migration.
        /// </summary>
        public static ContentTypeDefinitionBuilder SetAbilities(
            this ContentTypeDefinitionBuilder builder,
            bool creatable = false,
            bool listable = false,
            bool draftable = false,
            bool versionable = false,
            bool securable = false) =>
            builder.MergeSettings<ContentTypeSettings>(x =>
            {
                x.Creatable = creatable;
                x.Listable = listable;
                x.Draftable = draftable;
                x.Versionable = versionable;
                x.Securable = securable;
            });
    }
}
