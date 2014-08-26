namespace FakeItEasy.Core
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    [Serializable]
    public class NewEventRule : IFakeObjectCallRule
    {
        private readonly FakeManager fakeManager;
        private readonly Dictionary<EventInfo, Delegate> registeredHandlers = new Dictionary<EventInfo, Delegate>();

        public NewEventRule(FakeManager fakeManager)
        {
            this.fakeManager = fakeManager;
        }

        public int? NumberOfTimesToCall
        {
            get { return null; }
        }

        public bool IsApplicableTo(IFakeObjectCall fakeObjectCall)
        {
            return GetEvent(fakeObjectCall.Method) != null;
        }

        public void Apply(IInterceptedFakeObjectCall fakeObjectCall)
        {
            EventCall call = GetEventCall(fakeObjectCall);

            if (call.IsRegistration)
            {
                if (call.IsRaiser)
                {
                    this.RaiseEvent(call);
                }
                else
                {
                    this.AddHandler(call.Event, call.Handler);
                }
            }
        }

        private void RaiseEvent(EventCall call)
        {
            // With FakeItEasy's design I should raise the event here!
            // PROBLEM: I can't get all the info I need to raise the event (e.g. event args are missing), tried several things already.
            Delegate handler = this.registeredHandlers[call.Event];
            handler.DynamicInvoke(null, EventArgs.Empty);
        }

        private void AddHandler(EventInfo @event, Delegate handler)
        {
            this.registeredHandlers[@event] = handler;
        }

        private static EventCall GetEventCall(IInterceptedFakeObjectCall fakeObjectCall)
        {
            EventInfo eventInfo = GetEvent(fakeObjectCall.Method);

            return new EventCall(eventInfo, fakeObjectCall.Method, (Delegate)fakeObjectCall.Arguments[0]); 
        }

        public static EventInfo GetEvent(MethodInfo eventAdderOrRemover)
        {
            return eventAdderOrRemover.DeclaringType.GetEvents()
                .Where(e => 
                    object.Equals(e.GetAddMethod().GetBaseDefinition(), eventAdderOrRemover.GetBaseDefinition())
                    || object.Equals(e.GetRemoveMethod().GetBaseDefinition(), eventAdderOrRemover.GetBaseDefinition()))
                .Select(e => e).SingleOrDefault();
        }

    }

    public class EventCall
    {
        public EventCall(EventInfo @event, MethodInfo caller, Delegate handler)
        {
            this.Handler = handler;
            this.Event = @event;
            this.Caller = caller;
        }

        public EventInfo Event { get; private set; }

        public MethodInfo Caller { get; private set; }

        public Delegate Handler { get; private set; }

        public bool IsRegistration
        {
            get
            {
                return this.Event.GetAddMethod().Equals(this.Caller);
            }
        }

        public bool IsRaiser
        {
            get
            {
                return this.Handler == null;
            }
        }
    }
}