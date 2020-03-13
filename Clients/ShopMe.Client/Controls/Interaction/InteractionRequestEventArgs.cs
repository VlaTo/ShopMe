﻿using System;

namespace ShopMe.Client.Controls.Interaction
{
    public sealed class InteractionRequestEventArgs : EventArgs
    {
        public InteractionRequestContext Context
        {
            get;
        }

        public Action Callback
        {
            get;
        }

        public InteractionRequestEventArgs(InteractionRequestContext context, Action callback)
        {
            Context = context;
            Callback = callback;
        }
    }
}