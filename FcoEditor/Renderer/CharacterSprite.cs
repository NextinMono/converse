using libfco;
using Sprite = Converse.Rendering.Sprite;
namespace Converse.ShurikenRenderer
{
    public struct CharacterSprite
    {
        public Character converseChara;
        public int spriteId;
        public Sprite sprite => SpriteHelper.TryGetSprite(spriteId);
        public CharacterSprite(Character in_ConverseChara, int in_SpriteId)
        {
            converseChara = in_ConverseChara;
            spriteId = in_SpriteId;
        }
    }
}