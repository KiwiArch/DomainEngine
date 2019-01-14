﻿namespace Ode.Domain.Engine.Factories
{
    using Brokers;
    using Dispatchers;
    using Model;
    using Repositories;

    public static class DomainFactory
    {
        /// <summary>
        /// Creates a domin execution instance that can fully process command handlers and any configured event handlers
        /// including recursive handling of any commands generated by process managers.
        /// </summary>
        /// <param name="boundedContextModel">Describes the static model of the domain.  Include the command handlers and any event handlers to be executed in process.</param>
        /// <param name="eventStore">The event store used for event persistence by both command and event handers.</param>
        /// <param name="isRuntimeModelCached">Is the runtime moel cached between calls, only set this true for single instance deployments.</param>
        /// <returns>Domain inteface for processing commands.</returns>
        public static IDomainEngine CreateDomainExecutionEngine(IBoundedContextModel boundedContextModel, IEventStore eventStore, DomainOptions options)
        {
            return new DomainEngine(boundedContextModel, eventStore, options).WithEventBroker();


            //var domainEngine = new DomainExecutionEngine(boundedContextModel, eventStore);
            //domainEngine.IsRuntimeModelCached = isRuntimeModelCached;
            //return domainEngine;
        }

        /// <summary>
        /// Creates a domin execution instance that can fully process command handlers and any configured event handlers
        /// including recursive handling of any commands generated by process managers.
        /// </summary>
        /// <param name="boundedContextModel">Describes the static model of the domain.  Include the command handlers and any event handlers to be executed in process.</param>
        /// <param name="eventStore">The event store used for event persistence by both command and event handers.</param>
        /// <param name="eventQueueWriter">An event sink for writing the event output to a queue or similiar from within the command processing transaction.</param>
        /// <param name="isRuntimeModelCached">Is the runtime moel cached between calls, only set this true for single instance deployments.</param>
        /// <returns>Domain inteface for processing commands.</returns>
        public static IDomainEngine CreateDomainExecutionEngine(IBoundedContextModel boundedContextModel, IEventStore eventStore, IEventQueueWriter eventQueueWriter, DomainOptions options)
        {
            return new DomainEngine(boundedContextModel, eventStore, options).WithEventBroker().WithEventQueue(eventQueueWriter);

            //var domainEngine = new DomainExecutionEngine(boundedContextModel, eventStore).WithEventQueue(eventQueueWriter);
            //domainEngine.IsRuntimeModelCached = isRuntimeModelCached;
            //return domainEngine;
        }

        /// <summary>
        /// Creates a domain command engine that processes command handlers and returns any events created.
        /// </summary>
        /// <param name="boundedContextModel">Describes the static model of the domain.  Any defined event handlers will be ignored.</param>
        /// <param name="eventStore">The event store used for event persistence by command handlers.</param>
        /// <returns>Domain inteface for processing commands.</returns>
        public static IDomainEngine CreateCommandEngine(IBoundedContextModel boundedContextModel, IEventStore eventStore)
        {
            return new DomainEngine(boundedContextModel, eventStore, DomainOptions.Defaults);
        }

        /// <summary>
        /// Creates a domain command engine for processing command handlers and returns any events created as well as passing them to the provided event
        /// dispatcher while within the same transaction scope.
        /// </summary>
        /// <param name="boundedContextModel">Describes the static model of the domain.  Any defined event handlers will be ignored.</param>
        /// <param name="eventStore">The event store used for event persistence by command handlers.</param>
        /// <param name="eventDispatcher">The one way dispatcher  that dispatches events to their handlers. This will be called once for each event raised.</param>
        /// <returns>Domain inteface for processing commands.</returns>
        public static IDomainEngine CreateCommandEngine(IBoundedContextModel boundedContextModel, IEventStore eventStore, IEventDispatcher eventDispatcher)
        {
            return new DomainEngine(boundedContextModel, eventStore, DomainOptions.Defaults).WithEventDispatcher(eventDispatcher);
        }


        /// <summary>
        /// Creates a domain command engine for processing command handlers and returns any events created as well as passing them to the provided event
        /// broker while within the same transaction scope.
        /// </summary>
        /// <param name="boundedContextModel">Describes the static model of the domain.  Any defined event handlers will be ignored.</param>
        /// <param name="eventStore">The event store used for event persistence by command handlers.</param>
        /// <param name="eventBroker">The broker that issues events to their handlers and expects a response. This will be called once for each event raised.</param>
        /// <returns>Domain inteface for processing commands.</returns>
        public static IDomainEngine CreateCommandEngine(IBoundedContextModel boundedContextModel, IEventStore eventStore, IEventBroker eventBroker)
        {
            return new DomainEngine(boundedContextModel, eventStore, DomainOptions.Defaults).WithEventBroker(eventBroker);
        }

        /// <summary>
        /// Creates a domain command engine for processing command handlers that returns any events created as well as passing them to any defined event
        /// handlers while within the same transaction scope.  It is the responsibility of the event handers to further dispatch any commands they raise.
        /// </summary>
        /// <param name="boundedContextModel">Describes the static model of the domain.  Event handler definitions are used to determine the event handlers to call.</param>
        /// <param name="eventStore">The event store used for event persistence by command handlers.</param>
        /// <param name="eventHandler">The event handler instance to receive events.  This will be called once per event for each defined event handler.</param>
        /// <returns>Domain inteface for processing commands.</returns>
        public static IDomainEngine CreateCommandEngine(IBoundedContextModel boundedContextModel, IEventStore eventStore, IEventHandler eventHandler)
        {
            return new DomainEngine(boundedContextModel, eventStore, DomainOptions.Defaults).WithEventDispatcher(new EventDispatcher(boundedContextModel, eventHandler));
        }

        /// <summary>
        /// Creates a domain command engine for processing command handlers and dispatching resulting events to all the defined handlers and further dispatching
        /// any commands the event handlers raise on to the respective command handler, all within the same transaction scope.
        /// </summary>
        /// <param name="boundedContextModel">Describes the static model of the domain.  Event handler definitions are used to determine the event handlers to call.</param>
        /// <param name="eventStore">The event store used for event persistence by command handlers.</param>
        /// <param name="eventHandler">The event handler instance to receive events.  This will be called once per event for each defined event handler.</param>
        /// <param name="commandHandler">The command handler instance to recieve commands raised by event handlers. Called once for each command raised.</param>
        /// <returns>Domain inteface for processing commands.</returns>
        public static IDomainEngine CreateCommandEngine(IBoundedContextModel boundedContextModel, IEventStore eventStore, IEventHandler eventHandler, ICommandHandler commandHandler)
        {
            return new DomainEngine(boundedContextModel, eventStore, DomainOptions.Defaults).WithEventBroker(new DomainBroker(boundedContextModel, eventHandler, commandHandler));
        }

        /// <summary>
        /// Creates a domain event engine for processing an event handler.
        /// </summary>
        /// <param name="boundedContextModel">Describes the static model of the domain.  Only event handler definitions are used for the processing of events.</param>
        /// <param name="eventStore">The event store used to store any events handled by event handlers to ensure idempotency.</param>
        /// <returns>Domain inteface for processing events.</returns>
        public static IEventHandler CreateEventHandler(IBoundedContextModel boundedContextModel, IEventStore eventStore)
        {
            return new TransactionalEventHandler(boundedContextModel, eventStore, cacheRuntimeModel: false);
        }


        /// <summary>
        /// Created a dispatcher for domain events that can one way dispatch events to any defined event handlers.
        /// </summary>
        /// <param name="boundedContextModel">Describes the static model of the domain.  Only event handler definitions are used for determining the event handlers that need to b e called.</param>
        /// <param name="eventHandler">The event handler instance to receive events.  This will be called once per event for each defined event handler.</param>
        /// <returns>Domain inteface for dispatching events.</returns>
        public static IEventDispatcher CreateEventDispatcher(IBoundedContextModel boundedContextModel, IEventHandler eventHandler)
        {
            return new EventDispatcher(boundedContextModel, eventHandler);
        }

        public static IBoundedContextModel CreateBoundedContextModel()
        {
            return new BoundedContextModel();
        }
    }
}
