# RabbitMqIntegrationSample
Decoupling Scheme utilizing RabbitMq

Works with default guest/guest rabbit with default virtual host.

Creates queues for commands, exchanges for events.

To run: start SuperHeroBoundedContext, SuperVillainBoundedContext, ClientConsole - to send some igniting commands

*BoundedContext starts Host.exe and the later registers queues, exchanges, handlers.