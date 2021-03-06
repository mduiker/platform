﻿namespace GTANetwork.GUI.DirectXHook.Hook.Common
{
    public class TextElement: Element
    {
        public virtual string Text { get; set; }
        public virtual System.Drawing.Font Font { get; set; }
        public virtual System.Drawing.Color Color { get; set; }
        public virtual System.Drawing.Point Location { get; set; }
        public virtual bool AntiAliased { get; set; }

        public TextElement(System.Drawing.Font font)
        {
            Font = font;
        }
    }
}
