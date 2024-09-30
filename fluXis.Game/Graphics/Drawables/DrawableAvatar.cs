using System;
using fluXis.Game.Online;
using fluXis.Shared.Components.Users;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Framework.Input.Events;

namespace fluXis.Game.Graphics.Drawables;

#nullable enable

public partial class DrawableAvatar : Sprite
{
    [Resolved]
    private OnlineTextureStore store { get; set; } = null!;

    [Resolved]
    private TextureStore textures { get; set; } = null!;

    [Resolved]
    private UserCache? users { get; set; }

    public Action? ClickAction { get; init; }

    private APIUser? user;

    public DrawableAvatar(APIUser? user)
    {
        this.user = user;
        FillMode = FillMode.Fill;
    }

    [BackgroundDependencyLoader]
    private void load()
    {
        setTexture();
    }

    private void setTexture()
    {
        if (user is { ID: >= 0 }) // the texture from the online store could still be null
            Texture = store.GetAvatar(user.AvatarHash) ?? textures.Get("Online/default-avatar");
        else
            Texture = textures.Get("Online/default-avatar");

        Schedule(() => this.FadeInFromZero(400));
    }

    private void registerCallback()
    {
        if (user != null)
            users?.RegisterAvatarCallback(user.ID, reload);
    }

    private void unregisterCallback()
    {
        if (user != null)
            users?.UnregisterAvatarCallback(user.ID, reload);
    }

    private void reload()
    {
        // clear from texture store
        Texture = null;

        // wait 2 frames to allow texture store to clear
        Schedule(() => Schedule(setTexture));
    }

    public void UpdateUser(APIUser? newUser)
    {
        unregisterCallback();

        user = newUser;
        setTexture();

        registerCallback();
    }

    protected override bool OnClick(ClickEvent e)
    {
        ClickAction?.Invoke();
        return ClickAction != null;
    }

    protected override void Dispose(bool isDisposing)
    {
        base.Dispose(isDisposing);
        unregisterCallback();
    }
}
