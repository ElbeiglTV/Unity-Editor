# Event Channels sample

Open `EventChannelsSample.unity` and enter Play Mode.

`Event Raiser` sends `SampleEvent` from `Start`. `Event Listener` subscribes in
`OnEnable` and reads the `message` and `frame` values. Both steps appear in the Console.
