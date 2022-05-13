using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Fergun.Interactive;
using Fergun.Interactive.Pagination;

namespace Pointless.Utils
{
    public static class SocketInteractionContextExtensions
    {
        public static async Task RespondAsync(this SocketInteractionContext context, object content, bool ephemeral = false, bool disalbeMention = true, MessageComponent? component = null)
        {
            await context.Interaction.RespondAsync(content.ToString(), ephemeral: ephemeral, allowedMentions: disalbeMention ? AllowedMentions.None : null, components: component);
        }

        public static async Task RespondEmbedAsync(this SocketInteractionContext context, object content, bool ephemeral = false, bool disalbeMention = true, MessageComponent? component = null)
        {
            Embed emb = context.CreateEmbed(content.ToString()).Build();

            await context.Interaction.RespondAsync(embed: emb, ephemeral: ephemeral, allowedMentions: disalbeMention ? AllowedMentions.None : null, components: component);
        }

        public static async Task RespondEmbedAsync(this SocketInteractionContext context, Embed emb, string? content = null, bool ephemeral = false, bool disalbeMention = true, MessageComponent? component = null)
        {
            await context.Interaction.RespondAsync(content, embed: emb, ephemeral: ephemeral, allowedMentions: disalbeMention ? AllowedMentions.None : null, components: component);
        }

        public static EmbedBuilder CreateEmbed(this SocketInteractionContext context, object description = null, string title = null, string imgUrl = null, string url = null, string thumbnailUrl = null, Color? color = null)
        {
            return CreateEmbed(description, title, imgUrl, url, thumbnailUrl, color);
        }

        public static EmbedBuilder CreateEmbed(this SocketUser user, object description = null, string title = null, string imgUrl = null, string url = null, string thumbnailUrl = null, Color? color = null)
        {
            return CreateEmbed(description, title, imgUrl, url, thumbnailUrl, color);
        }

        public static EmbedBuilder CreateEmbed(object description = null, string title = null, string imgUrl = null, string url = null, string thumbnailUrl = null, Color? color = null)
        {
            EmbedBuilder emb = new()
            {
                Title = title,
                Color = color ?? new Color(255, 200, 0),
                Description = description?.ToString(),
                ImageUrl = imgUrl,
                Url = url,
                ThumbnailUrl = thumbnailUrl,
            };

            return emb;
        }

        public static PageBuilder CreatePage(this SocketInteractionContext context, object description = null, string title = null, string imgUrl = null, string url = null, string thumbnailUrl = null, Color? color = null)
        {
            return CreatePage(context.User, context.Interaction.IsDMInteraction, description, title, imgUrl, url, thumbnailUrl, color);
        }

        public static PageBuilder CreatePage(this SocketUser user, bool isPrivate, object description = null, string title = null, string imgUrl = null, string url = null, string thumbnailUrl = null, Color? color = null)
        {
            PageBuilder page = new()
            {
                Title = title,
                Color = color ?? new Color(255, 200, 0),
                /* Footer = new()
                {
                    Text = user.GetName(isPrivate),
                    IconUrl = user.GetAvatar()
                }, */
                Description = description?.ToString(),
                ImageUrl = imgUrl,
                Url = url,
                ThumbnailUrl = thumbnailUrl,
            };

            return page;
        }

        public static EmbedBuilder AddEmptyField(this EmbedBuilder emb)
        {
            return emb.AddField("**  **", "** **", true);
        }

        public static LazyPaginator CreatePagenator(this SocketInteractionContext context, int count, Func<int, IPageBuilder> factory)
        {
            return CreatePagenator(context.User, count, factory);
        }

        public static LazyPaginator CreatePagenator(this IUser user, int count, Func<int, IPageBuilder> factory)
        {
            return new LazyPaginatorBuilder()
                .AddUser(user)
                .AddOption(new Emoji("◀"), PaginatorAction.Backward)
                .AddOption(new Emoji("▶"), PaginatorAction.Forward)
                .WithPageFactory(factory)
                .WithMaxPageIndex(count)
                .WithActionOnTimeout(ActionOnStop.DisableInput)
                .WithCacheLoadedPages(false)
                .Build();
        }
    }
}
