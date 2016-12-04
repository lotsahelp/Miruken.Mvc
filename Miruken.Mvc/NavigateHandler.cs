﻿using System;
using SixFlags.CF.Miruken.Callback;
using SixFlags.CF.Miruken.Concurrency;
using SixFlags.CF.Miruken.Container;
using SixFlags.CF.Miruken.Context;
using SixFlags.CF.Miruken.MVC.Options;
using SixFlags.CF.Miruken.MVC.Views;

namespace SixFlags.CF.Miruken.MVC
{
    public class NavigateHandler : CompositeCallbackHandler, INavigate
    {
        public NavigateHandler(IViewRegion mainRegion)
        {
            AddHandlers(mainRegion);
        }

        Promise<IContext> INavigate.Next<C>(Action<C> action)
        {
            return ((INavigate)this).Navigate(action, NavigationStyle.Next);
        }

        Promise<IContext> INavigate.Push<C>(Action<C> action)
        {
            return ((INavigate)this).Navigate(action, NavigationStyle.Push);
        }

        Promise<IContext> INavigate.Part<C>(Action<C> action)
        {
            return ((INavigate)this).Navigate(action, NavigationStyle.Part);
        }

        Promise<IContext> INavigate.Navigate<C>(Action<C> action, NavigationStyle navStyle)
        {
            var composer = Composer;
            if (action == null || composer == null) return null;

            var context   = composer.Resolve<IContext>();
            var initiator = composer.Resolve<IController>();

            var ctx = navStyle != NavigationStyle.Next
                    ? context.CreateChild()
                    : context;

            C controller;
            try
            {
                controller = (C)ResolveController(ctx, typeof(C));
                if (initiator != null && initiator.Context != ctx)
                    initiator.DependsOn(controller);
            }
            catch (Exception exception)
            {
                if (navStyle != NavigationStyle.Next)
                    ctx.End();
                return Promise<IContext>.Rejected(exception);
            }

            var oldIO = Controller._io;

            try
            {
                if (navStyle == NavigationStyle.Push)
                    composer = composer.PushLayer();
                else
                {
                    var ctrl = controller as Controller;
                    var init = initiator as Controller;
                    if (ctrl != null && init != null)
                    {
                        ctrl._lastAction  = h => new INavigate(h).Next(action);
                        ctrl._retryAction = init._lastAction;
                    }
                }

                // Propogate handler options
                Controller._io = ctx.Chain(composer);

                action(controller);

                if (initiator != null && initiator.Context == ctx)
                    initiator.Release();
            }
            catch (Exception exception)
            {
                if (navStyle != NavigationStyle.Next)
                    ctx.End();
                else if (initiator != null && initiator.Context == ctx)
                    controller.DependsOn(initiator);
                return Promise<IContext>.Rejected(exception);
            }
            finally
            {
                Controller._io = oldIO;
            }

            return Promise.Resolved(ctx);
        }

        Promise<IContext> INavigate.GoBack()
        {
            var composer = Composer;
            if (composer == null) return null;
            var controller = composer.Resolve<Controller>();
            return (controller != null && controller._retryAction != null)
                 ? controller._retryAction(composer)
                 : null;
        }

        private static IController ResolveController(IContext context, Type type)
        {
            var controller = (IController)new IContainer(context).Resolve(type);
            context.ContextEnded += _ => controller.Release();
            controller.Policy.AutoRelease();
            controller.Context = context;
            return controller;
        }
    }
}