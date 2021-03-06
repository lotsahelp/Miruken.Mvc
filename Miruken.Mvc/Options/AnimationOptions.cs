﻿using System;
using Miruken.Callback;

namespace Miruken.Mvc.Options
{
    public enum AnimationEffect
    {
        None = 0,
        MoveLeft,
        MoveRight,
        MoveUp,
        MoveDown,
        PushLeft,
        PushRight,
        PushUp,
        PushDown,
        CoverLeft,
        CoverRight,
        CoverUp,
        CoverDown,
        UncoverLeft,
        UncoverRight,
        UncoverUp,
        UncoverDown,
        AlphaBlend
    }

    public class AnimationOptions : CallbackOptions<AnimationOptions>
    {
        public AnimationEffect? Effect   { get; set; }
        public double?          Duration { get; set; }

        public AnimationEffect? InverseEffect
        {
            get
            {
                if (!Effect.HasValue) return null;
                switch (Effect)
                {
                    case AnimationEffect.MoveLeft:
                        return AnimationEffect.MoveRight;
                    case AnimationEffect.MoveRight:
                        return AnimationEffect.MoveLeft;
                    case AnimationEffect.MoveUp:
                        return AnimationEffect.MoveDown;
                    case AnimationEffect.MoveDown:
                        return AnimationEffect.MoveUp;
                    case AnimationEffect.PushLeft:
                        return AnimationEffect.PushRight;
                    case AnimationEffect.PushRight:
                        return AnimationEffect.PushLeft;
                    case AnimationEffect.PushUp:
                        return AnimationEffect.PushDown;
                    case AnimationEffect.PushDown:
                        return AnimationEffect.PushUp;
                    case AnimationEffect.CoverLeft:
                        return AnimationEffect.UncoverRight;
                    case AnimationEffect.CoverRight:
                        return AnimationEffect.UncoverLeft;
                    case AnimationEffect.CoverUp:
                        return AnimationEffect.UncoverDown;
                    case AnimationEffect.CoverDown:
                        return AnimationEffect.UncoverUp;
                    case AnimationEffect.UncoverLeft:
                        return AnimationEffect.CoverRight;
                    case AnimationEffect.UncoverRight:
                        return AnimationEffect.CoverLeft;
                    case AnimationEffect.UncoverUp:
                        return AnimationEffect.CoverDown;
                    case AnimationEffect.UncoverDown:
                        return AnimationEffect.CoverUp;
                }
                return Effect;
            }
        }

        public override void MergeInto(AnimationOptions other)
        {
            if (Effect.HasValue && !other.Effect.HasValue)
                other.Effect = Effect;

            if (Duration.HasValue && !other.Duration.HasValue)
                other.Duration = Duration;
        }
    }

    public static class AnimationOptionsExtensions
    {
        public static IHandler Animate(
            this IHandler handler, AnimationEffect effect)
        {
            return new RegionOptions
            {
                Animation = new AnimationOptions
                {
                    Effect = effect
                }
            }.Decorate(handler);
        }

        public static IHandler Animate(
            this IHandler handler, AnimationEffect effect, double duration)
        {
            return new RegionOptions
            {
                Animation = new AnimationOptions
                {
                    Effect = effect,
                    Duration = duration
                }
            }.Decorate(handler);
        }

        public static IHandler Animate(
             this IHandler handler, Action<AnimationBuilder> build)
        {
            if (handler == null || build == null)
                return handler;
            var builder = new AnimationBuilder();
            build(builder);
            return new RegionOptions
            {
                Animation = builder.AnimationOptions
            }.Decorate(handler);
        }
    }
}
