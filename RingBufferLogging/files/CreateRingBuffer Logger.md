
# Instructions

- You are a skilled and experienced .net developer.
- I need you to create a ring buffer logger.
- This logger will accumulate structured log messages in a ring buffer, allowing for efficient logging without excessive memory usage.
- This will be a class that decorates an existing I logger interface.
- The ring buffer will have a fixed size, and when it reaches capacity, it will overwrite the oldest log messages.
- The backing store for this ring buffer is a redis database
- Each entry in the ring buffer is a key in the redis database, with the log message as the value.
- There can be multiple instances of this Ring Buffer, each with its own key prefix in the redis database.
- The class should implement the `ILogger` interface and provide methods for logging messages at different levels (e.g., Information, Warning, Error).
- The class should also provide a method to retrieve the current log messages from the ring buffer.
- The class should handle exceptions gracefully and log them to the console or a fallback logger if necessary.
- The class should be thread-safe to allow concurrent logging from multiple threads.
- The class should be configurable, allowing the user to set the size of the ring buffer and the key prefix for the redis database.
- The class should be unit testable, with appropriate interfaces and abstractions to allow for mocking dependencies.
- The class should be documented with XML comments for public methods and properties.
- The class should be designed with performance in mind, minimizing the overhead of logging operations.
- The class should be compatible with .NET 6 and later versions.
- The class should be implemented in C# and follow best practices for .NET development.
- The class should be structured in a way that allows for easy extension and modification in the future.
- The class should include a method to clear the ring buffer, allowing for resetting the log messages.
- The class should include a method to retrieve the size of the ring buffer and the number of log messages currently stored.
- The current position of the ringbuffer will also be stored in the redis database, allowing for retreiving the logn entries in te order they were logged
- The class should include a method to retrieve the log messages in the order they were logged, based on the current position stored in the redis database.

