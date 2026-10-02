# AvaEnyo

AvaEnyo is a webOS Enyo 1.0 Onyx theme for Avalonia, designed from parsing the [source code](https://github.com/enyojs/enyo-1.0) and converting it to Avalonia controls, themes, and animations.

## Getting started

```xml
<Application xmlns="https://github.com/avaloniaui"
             xmlns:enyo="https://github.com/avaenyo">
  <Application.Styles>
    <enyo:AvaEnyoTheme Density="Default" />   <!-- Default | Compact -->
    <enyo:AvaEnyoControlsTheme />             <!-- when AvaEnyo.Controls is referenced -->
    <enyo:AvaEnyoServicesTheme />             <!-- when AvaEnyo.Services is referenced -->
  </Application.Styles>
</Application>
```

```csharp
AppBuilder.Configure<App>()
    .UsePlatformDetect()
    .WithAvaEnyoFonts()          // registers the bundled Source Sans 3
    .StartWithClassicDesktopLifetime(args);
```

A three-panel app:

```xml
<enyo:SlidingPane>
  <enyo:SlidingView Width="320">
    <ScrollViewer>
      <StackPanel>
        <enyo:Divider Caption="Accounts" />
        <enyo:Item Content="Inbox" />
      </StackPanel>
    </ScrollViewer>
  </enyo:SlidingView>
  <enyo:SlidingView Width="320" FixedWidth="True">
    <enyo:VirtualList ItemsSource="{Binding Messages}" Swipeable="True" />
  </enyo:SlidingView>
  <enyo:SlidingView enyo:Flex.Value="1" Dismissible="True">
    <DockPanel>
      <enyo:Toolbar DockPanel.Dock="Bottom">
        <enyo:GrabButton />
        <enyo:Spacer />
        <enyo:ToolButton Content="Reply" />
      </enyo:Toolbar>
      <ScrollViewer><TextBlock Text="{Binding Body}" TextWrapping="Wrap" /></ScrollViewer>
    </DockPanel>
  </enyo:SlidingView>
</enyo:SlidingPane>
```