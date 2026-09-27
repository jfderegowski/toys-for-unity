using System;
using UnityEngine;

namespace fefek5.Toys.Runtime.Extensions
{
    public static class Texture2DExtensions
    {
        /// <summary>
        /// Creates a sprite covering the whole texture.
        /// <para>
        /// Unlike the short <see cref="Sprite.Create(Texture2D, Rect, Vector2)"/> overload, this defaults to
        /// <see cref="SpriteMeshType.FullRect"/>. The short overload builds a tight mesh, tracing the outline
        /// of every opaque pixel, which costs milliseconds per texture — enough to stall a frame when
        /// creating sprites for a list of avatars or downloaded icons — and gains nothing for images drawn as
        /// rectangles, such as UI <c>Image</c>s. Pass <see cref="SpriteMeshType.Tight"/> only for sprites with
        /// large transparent areas rendered by a <c>SpriteRenderer</c>.
        /// </para>
        /// </summary>
        /// <param name="texture">The texture this method extends.</param>
        /// <param name="pixelsPerUnit">Pixels in one world unit. Default is 100, the same as Unity's.</param>
        /// <param name="pivot">Pivot relative to the texture size. Default is the center (0.5, 0.5).</param>
        /// <param name="meshType">Mesh generated for the sprite. Default is <see cref="SpriteMeshType.FullRect"/>.</param>
        /// <returns>A new sprite. It is not destroyed with the texture; destroy it when no longer needed.</returns>
        public static Sprite ToSprite(this Texture2D texture, float pixelsPerUnit = 100f, Vector2? pivot = null,
            SpriteMeshType meshType = SpriteMeshType.FullRect)
        {
            if (!texture) throw new ArgumentNullException(nameof(texture));

            return Sprite.Create(
                texture,
                new Rect(0f, 0f, texture.width, texture.height),
                pivot ?? new Vector2(0.5f, 0.5f),
                pixelsPerUnit,
                0,
                meshType);
        }
    }
}
