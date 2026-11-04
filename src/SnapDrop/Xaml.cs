namespace SnapDrop
{
    /// <summary>All UI layouts live here as XAML strings (parsed at runtime, so no XAML compiler is needed).</summary>
    static class Xaml
    {
        public const string Styles = """
<Style x:Key="ToolBtn" TargetType="RadioButton">
  <Setter Property="Foreground" Value="#FFE8EAED"/>
  <Setter Property="FontSize" Value="13"/>
  <Setter Property="Margin" Value="2,0"/>
  <Setter Property="Cursor" Value="Hand"/>
  <Setter Property="Template">
    <Setter.Value>
      <ControlTemplate TargetType="RadioButton">
        <Border x:Name="B" CornerRadius="7" Padding="11,6" Background="Transparent">
          <ContentPresenter HorizontalAlignment="Center" VerticalAlignment="Center"/>
        </Border>
        <ControlTemplate.Triggers>
          <Trigger Property="IsMouseOver" Value="True"><Setter TargetName="B" Property="Background" Value="#26FFFFFF"/></Trigger>
          <Trigger Property="IsChecked" Value="True"><Setter TargetName="B" Property="Background" Value="#FF3B82F6"/></Trigger>
        </ControlTemplate.Triggers>
      </ControlTemplate>
    </Setter.Value>
  </Setter>
</Style>
<Style x:Key="Btn" TargetType="Button">
  <Setter Property="Foreground" Value="#FFE8EAED"/>
  <Setter Property="FontSize" Value="13"/>
  <Setter Property="Margin" Value="3,0"/>
  <Setter Property="Cursor" Value="Hand"/>
  <Setter Property="Template">
    <Setter.Value>
      <ControlTemplate TargetType="Button">
        <Border x:Name="B" CornerRadius="7" Padding="12,6" Background="#FF37393F">
          <ContentPresenter HorizontalAlignment="Center" VerticalAlignment="Center"/>
        </Border>
        <ControlTemplate.Triggers>
          <Trigger Property="IsMouseOver" Value="True"><Setter TargetName="B" Property="Background" Value="#FF484B53"/></Trigger>
          <Trigger Property="IsPressed" Value="True"><Setter TargetName="B" Property="Background" Value="#FF2C2E33"/></Trigger>
          <Trigger Property="IsEnabled" Value="False"><Setter Property="Opacity" Value="0.4"/></Trigger>
        </ControlTemplate.Triggers>
      </ControlTemplate>
    </Setter.Value>
  </Setter>
</Style>
<Style x:Key="AccentBtn" TargetType="Button" BasedOn="{StaticResource Btn}">
  <Setter Property="Foreground" Value="White"/>
  <Setter Property="FontWeight" Value="SemiBold"/>
  <Setter Property="Template">
    <Setter.Value>
      <ControlTemplate TargetType="Button">
        <Border x:Name="B" CornerRadius="7" Padding="16,6" Background="#FF3B82F6">
          <ContentPresenter HorizontalAlignment="Center" VerticalAlignment="Center"/>
        </Border>
        <ControlTemplate.Triggers>
          <Trigger Property="IsMouseOver" Value="True"><Setter TargetName="B" Property="Background" Value="#FF5595F8"/></Trigger>
          <Trigger Property="IsPressed" Value="True"><Setter TargetName="B" Property="Background" Value="#FF2F6FD8"/></Trigger>
        </ControlTemplate.Triggers>
      </ControlTemplate>
    </Setter.Value>
  </Setter>
</Style>
<Style x:Key="Swatch" TargetType="RadioButton">
  <Setter Property="Cursor" Value="Hand"/>
  <Setter Property="Template">
    <Setter.Value>
      <ControlTemplate TargetType="RadioButton">
        <Grid Margin="2" Width="24" Height="24" Background="Transparent">
          <Ellipse x:Name="Ring" Stroke="White" StrokeThickness="2" Opacity="0"/>
          <Ellipse Margin="4" Fill="{TemplateBinding Background}" Stroke="#66FFFFFF" StrokeThickness="1"/>
        </Grid>
        <ControlTemplate.Triggers>
          <Trigger Property="IsMouseOver" Value="True"><Setter TargetName="Ring" Property="Opacity" Value="0.5"/></Trigger>
          <Trigger Property="IsChecked" Value="True"><Setter TargetName="Ring" Property="Opacity" Value="1"/></Trigger>
        </ControlTemplate.Triggers>
      </ControlTemplate>
    </Setter.Value>
  </Setter>
</Style>
<Style x:Key="Sep" TargetType="Rectangle">
  <Setter Property="Width" Value="1"/>
  <Setter Property="Height" Value="22"/>
  <Setter Property="Margin" Value="10,0"/>
  <Setter Property="Fill" Value="#44FFFFFF"/>
</Style>
<Style x:Key="CloseBtn" TargetType="Button">
  <Setter Property="Cursor" Value="Hand"/>
  <Setter Property="Template">
    <Setter.Value>
      <ControlTemplate TargetType="Button">
        <Border x:Name="B" Width="22" Height="22" CornerRadius="11" Background="#CC000000" BorderBrush="#55FFFFFF" BorderThickness="1">
          <TextBlock Text="✕" Foreground="White" FontSize="10" HorizontalAlignment="Center" VerticalAlignment="Center"/>
        </Border>
        <ControlTemplate.Triggers>
          <Trigger Property="IsMouseOver" Value="True"><Setter TargetName="B" Property="Background" Value="#FFE5484D"/></Trigger>
        </ControlTemplate.Triggers>
      </ControlTemplate>
    </Setter.Value>
  </Setter>
</Style>
<Style x:Key="NavBtn" TargetType="RadioButton">
  <Setter Property="Foreground" Value="#FFC9CDD3"/>
  <Setter Property="FontSize" Value="14"/>
  <Setter Property="Margin" Value="0,2"/>
  <Setter Property="Cursor" Value="Hand"/>
  <Setter Property="Template">
    <Setter.Value>
      <ControlTemplate TargetType="RadioButton">
        <Border x:Name="B" CornerRadius="8" Padding="14,10" Background="Transparent">
          <Grid>
            <Border x:Name="Bar" Width="3" CornerRadius="2" Background="#FF3B82F6" HorizontalAlignment="Left" Margin="-10,2,0,2" Opacity="0"/>
            <ContentPresenter VerticalAlignment="Center"/>
          </Grid>
        </Border>
        <ControlTemplate.Triggers>
          <Trigger Property="IsMouseOver" Value="True"><Setter TargetName="B" Property="Background" Value="#1AFFFFFF"/></Trigger>
          <Trigger Property="IsChecked" Value="True">
            <Setter TargetName="B" Property="Background" Value="#26FFFFFF"/>
            <Setter TargetName="Bar" Property="Opacity" Value="1"/>
            <Setter Property="Foreground" Value="White"/>
          </Trigger>
        </ControlTemplate.Triggers>
      </ControlTemplate>
    </Setter.Value>
  </Setter>
</Style>
<Style x:Key="Switch" TargetType="CheckBox">
  <Setter Property="Cursor" Value="Hand"/>
  <Setter Property="VerticalAlignment" Value="Center"/>
  <Setter Property="Template">
    <Setter.Value>
      <ControlTemplate TargetType="CheckBox">
        <Grid Width="44" Height="24" Background="Transparent">
          <Border x:Name="Track" CornerRadius="12" Background="#FF3A3D44"/>
          <Ellipse x:Name="Knob" Width="18" Height="18" Fill="White" HorizontalAlignment="Left" Margin="3,0,0,0"/>
        </Grid>
        <ControlTemplate.Triggers>
          <Trigger Property="IsChecked" Value="True">
            <Setter TargetName="Track" Property="Background" Value="#FF3B82F6"/>
            <Setter TargetName="Knob" Property="HorizontalAlignment" Value="Right"/>
            <Setter TargetName="Knob" Property="Margin" Value="0,0,3,0"/>
          </Trigger>
          <Trigger Property="IsMouseOver" Value="True"><Setter TargetName="Track" Property="Opacity" Value="0.88"/></Trigger>
        </ControlTemplate.Triggers>
      </ControlTemplate>
    </Setter.Value>
  </Setter>
</Style>
<Style x:Key="Field" TargetType="TextBox">
  <Setter Property="Foreground" Value="#FFE8EAED"/>
  <Setter Property="CaretBrush" Value="White"/>
  <Setter Property="FontSize" Value="13"/>
  <Setter Property="Padding" Value="10,7"/>
  <Setter Property="SelectionBrush" Value="#FF3B82F6"/>
  <Setter Property="Template">
    <Setter.Value>
      <ControlTemplate TargetType="TextBox">
        <Border x:Name="B" CornerRadius="7" Background="#FF26272B" BorderBrush="#FF3A3D44" BorderThickness="1">
          <ScrollViewer x:Name="PART_ContentHost" Margin="{TemplateBinding Padding}"/>
        </Border>
        <ControlTemplate.Triggers>
          <Trigger Property="IsKeyboardFocused" Value="True"><Setter TargetName="B" Property="BorderBrush" Value="#FF3B82F6"/></Trigger>
        </ControlTemplate.Triggers>
      </ControlTemplate>
    </Setter.Value>
  </Setter>
</Style>
<Style x:Key="Label" TargetType="TextBlock">
  <Setter Property="Foreground" Value="#FFF1F3F5"/>
  <Setter Property="FontSize" Value="14"/>
  <Setter Property="FontWeight" Value="SemiBold"/>
</Style>
<Style x:Key="Hint" TargetType="TextBlock">
  <Setter Property="Foreground" Value="#FF8E949C"/>
  <Setter Property="FontSize" Value="12"/>
  <Setter Property="TextWrapping" Value="Wrap"/>
  <Setter Property="Margin" Value="0,2,0,0"/>
</Style>
<Style x:Key="Heading" TargetType="TextBlock">
  <Setter Property="Foreground" Value="White"/>
  <Setter Property="FontSize" Value="22"/>
  <Setter Property="FontWeight" Value="SemiBold"/>
  <Setter Property="Margin" Value="0,0,0,18"/>
</Style>
""";

        public const string Thumb = """
<Window xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        WindowStyle="None" AllowsTransparency="True" Background="Transparent" ResizeMode="NoResize"
        ShowInTaskbar="False" Topmost="True" ShowActivated="False" UseLayoutRounding="True">
  <Window.Resources>{STYLES}</Window.Resources>
  <Grid Margin="14">
    <Border x:Name="Card" CornerRadius="12" Background="#F01E1F22" BorderBrush="#40FFFFFF" BorderThickness="1" Padding="6" Cursor="Hand">
      <Border.Effect><DropShadowEffect BlurRadius="18" ShadowDepth="4" Opacity="0.55" Color="Black"/></Border.Effect>
      <Grid>
        <Image x:Name="Pic" Stretch="Fill" RenderOptions.BitmapScalingMode="HighQuality"/>
        <Border x:Name="Hint" VerticalAlignment="Bottom" Opacity="0" Padding="6,16,6,5" IsHitTestVisible="False">
          <Border.Background>
            <LinearGradientBrush StartPoint="0,0" EndPoint="0,1">
              <GradientStop Color="#00000000" Offset="0"/><GradientStop Color="#D0000000" Offset="1"/>
            </LinearGradientBrush>
          </Border.Background>
          <TextBlock Text="Drag anywhere · Click to edit" Foreground="White" FontSize="11" HorizontalAlignment="Center"/>
        </Border>
        <Button x:Name="CloseBtn" Style="{StaticResource CloseBtn}" HorizontalAlignment="Left" VerticalAlignment="Top" Margin="5" Opacity="0"/>
      </Grid>
    </Border>
  </Grid>
</Window>
""";

        public const string Bar = """
<Window xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        WindowStyle="None" AllowsTransparency="True" Background="Transparent" ResizeMode="NoResize"
        ShowInTaskbar="False" Topmost="True" SizeToContent="WidthAndHeight" UseLayoutRounding="True">
  <Window.Resources>{STYLES}</Window.Resources>
  <Border Margin="14" CornerRadius="14" Background="#F2212225" BorderBrush="#40FFFFFF" BorderThickness="1" Padding="10,8">
    <Border.Effect><DropShadowEffect BlurRadius="20" ShadowDepth="4" Opacity="0.55" Color="Black"/></Border.Effect>
    <StackPanel Orientation="Horizontal">
      <Button x:Name="BScreen" Content="Entire screen" Style="{StaticResource Btn}"/>
      <Button x:Name="BWindow" Content="Window" Style="{StaticResource Btn}"/>
      <Button x:Name="BRegion" Content="Selection" Style="{StaticResource AccentBtn}"/>
      <Rectangle Style="{StaticResource Sep}"/>
      <TextBlock Text="Timer" Foreground="#FF9AA0A6" FontSize="12" VerticalAlignment="Center" Margin="0,0,4,0"/>
      <RadioButton x:Name="T0" Tag="0" GroupName="t" Content="Off" Style="{StaticResource ToolBtn}"/>
      <RadioButton x:Name="T3" Tag="3" GroupName="t" Content="3s" Style="{StaticResource ToolBtn}"/>
      <RadioButton x:Name="T5" Tag="5" GroupName="t" Content="5s" Style="{StaticResource ToolBtn}"/>
      <RadioButton x:Name="T10" Tag="10" GroupName="t" Content="10s" Style="{StaticResource ToolBtn}"/>
      <Rectangle Style="{StaticResource Sep}"/>
      <Button x:Name="BOpts" Content="Options ▾" Style="{StaticResource Btn}"/>
      <Button x:Name="BClose" Content="✕" Style="{StaticResource Btn}"/>
    </StackPanel>
  </Border>
</Window>
""";

        public const string Editor = """
<Window xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        Title="SnapDrop – Edit screenshot" Width="1180" Height="780" MinWidth="820" MinHeight="440"
        Background="#FF17181B" WindowStartupLocation="CenterScreen" UseLayoutRounding="True">
  <Window.Resources>{STYLES}</Window.Resources>
  <DockPanel>
    <Border DockPanel.Dock="Top" Background="#FF232428" Padding="10,8" BorderBrush="#FF34363B" BorderThickness="0,0,0,1">
      <DockPanel LastChildFill="False">
        <StackPanel DockPanel.Dock="Right" Orientation="Horizontal">
          <Border x:Name="DragChip" CornerRadius="7" Padding="12,6" Margin="3,0" Background="#FF2B3A55" BorderBrush="#FF3B82F6" BorderThickness="1" Cursor="SizeAll"
                  ToolTip="Drag this into any app, chat or folder. Your edits are included.">
            <TextBlock Text="⠿  Drag out" Foreground="#FFBBD3FF" FontSize="13"/>
          </Border>
          <Button x:Name="CopyBtn" Content="Copy" Style="{StaticResource Btn}"/>
          <Button x:Name="SaveAsBtn" Content="Save as…" Style="{StaticResource Btn}"/>
          <Button x:Name="DoneBtn" Content="Done" Style="{StaticResource AccentBtn}" ToolTip="Save over the original and close (Ctrl+S)"/>
        </StackPanel>
        <StackPanel Orientation="Horizontal">
          <RadioButton x:Name="TCrop"  Tag="crop"  GroupName="tool" Content="✂ Crop"      Style="{StaticResource ToolBtn}"/>
          <RadioButton x:Name="TPen"   Tag="pen"   GroupName="tool" Content="✎ Draw"      Style="{StaticResource ToolBtn}"/>
          <RadioButton x:Name="THi"    Tag="hi"    GroupName="tool" Content="▬ Highlight" Style="{StaticResource ToolBtn}"/>
          <RadioButton x:Name="TArrow" Tag="arrow" GroupName="tool" Content="↗ Arrow"     Style="{StaticResource ToolBtn}"/>
          <RadioButton x:Name="TBox"   Tag="box"   GroupName="tool" Content="▭ Box"       Style="{StaticResource ToolBtn}"/>
          <RadioButton x:Name="TText"  Tag="text"  GroupName="tool" Content="T Text"      Style="{StaticResource ToolBtn}"/>
          <RadioButton x:Name="TBlur"  Tag="blur"  GroupName="tool" Content="▦ Blur"      Style="{StaticResource ToolBtn}" ToolTip="Pixelate sensitive info"/>
          <Rectangle Style="{StaticResource Sep}"/>
          <StackPanel x:Name="Colors" Orientation="Horizontal" VerticalAlignment="Center"/>
          <Rectangle Style="{StaticResource Sep}"/>
          <StackPanel x:Name="Sizes" Orientation="Horizontal" VerticalAlignment="Center"/>
          <Rectangle Style="{StaticResource Sep}"/>
          <Button x:Name="UndoBtn" Content="↶" Style="{StaticResource Btn}" ToolTip="Undo (Ctrl+Z)"/>
          <Button x:Name="RedoBtn" Content="↷" Style="{StaticResource Btn}" ToolTip="Redo (Ctrl+Y)"/>
        </StackPanel>
      </DockPanel>
    </Border>
    <Border DockPanel.Dock="Bottom" Background="#FF232428" Padding="14,7" BorderBrush="#FF34363B" BorderThickness="0,1,0,0">
      <TextBlock x:Name="Status" Foreground="#FF9AA0A6" FontSize="12"/>
    </Border>
    <Grid x:Name="Stage" Background="#FF0F1012" ClipToBounds="True">
      <Viewbox x:Name="VB" Stretch="Uniform" StretchDirection="DownOnly" Margin="28" HorizontalAlignment="Center" VerticalAlignment="Center">
        <Grid x:Name="Wrap">
          <Grid x:Name="Holder" ClipToBounds="True">
            <Image x:Name="BaseImg" Stretch="Fill" RenderOptions.BitmapScalingMode="HighQuality"/>
            <Canvas x:Name="Ann"/>
          </Grid>
          <Canvas x:Name="Over" Background="Transparent"/>
        </Grid>
      </Viewbox>
      <Border x:Name="CropBar" VerticalAlignment="Bottom" HorizontalAlignment="Center" Margin="0,0,0,22" Visibility="Collapsed"
              CornerRadius="12" Background="#F2212225" BorderBrush="#40FFFFFF" BorderThickness="1" Padding="8">
        <StackPanel Orientation="Horizontal">
          <Button x:Name="CropCancel" Content="Cancel  (Esc)" Style="{StaticResource Btn}"/>
          <Button x:Name="CropApply" Content="Apply crop  (Enter)" Style="{StaticResource AccentBtn}"/>
        </StackPanel>
      </Border>
    </Grid>
  </DockPanel>
</Window>
""";
        public const string SettingsWindow = """
<Window xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        Title="SnapDrop Settings" Width="800" Height="620" MinWidth="720" MinHeight="520"
        Background="#FF17181B" WindowStartupLocation="CenterScreen" UseLayoutRounding="True" TextOptions.TextFormattingMode="Display">
  <Window.Resources>{STYLES}</Window.Resources>
  <Grid>
    <Grid.ColumnDefinitions><ColumnDefinition Width="220"/><ColumnDefinition Width="*"/></Grid.ColumnDefinitions>
    <Border Grid.Column="0" Background="#FF1E1F23" BorderBrush="#FF2C2E33" BorderThickness="0,0,1,0">
      <DockPanel Margin="18,22,18,18">
        <StackPanel DockPanel.Dock="Top" Orientation="Horizontal" Margin="4,0,0,26">
          <Image x:Name="BrandLogo" Width="36" Height="36" RenderOptions.BitmapScalingMode="HighQuality"/>
          <StackPanel Margin="12,0,0,0" VerticalAlignment="Center">
            <TextBlock Text="SnapDrop" Foreground="White" FontSize="18" FontWeight="SemiBold"/>
            <TextBlock x:Name="BrandVersion" Foreground="#FF8E949C" FontSize="12"/>
          </StackPanel>
        </StackPanel>
        <StackPanel>
          <RadioButton x:Name="NavGeneral"   GroupName="nav" Content="General"   Style="{StaticResource NavBtn}" IsChecked="True"/>
          <RadioButton x:Name="NavShortcuts" GroupName="nav" Content="Shortcuts" Style="{StaticResource NavBtn}"/>
          <RadioButton x:Name="NavThumb"     GroupName="nav" Content="Thumbnail" Style="{StaticResource NavBtn}"/>
          <RadioButton x:Name="NavAbout"     GroupName="nav" Content="About"     Style="{StaticResource NavBtn}"/>
        </StackPanel>
      </DockPanel>
    </Border>
    <DockPanel Grid.Column="1">
      <Border DockPanel.Dock="Bottom" Background="#FF1E1F23" BorderBrush="#FF2C2E33" BorderThickness="0,1,0,0" Padding="22,12">
        <DockPanel>
          <StackPanel DockPanel.Dock="Right" Orientation="Horizontal">
            <Button x:Name="BtnCancel" Content="Cancel" Style="{StaticResource Btn}"/>
            <Button x:Name="BtnSave" Content="Save" Style="{StaticResource AccentBtn}" Margin="6,0,0,0"/>
          </StackPanel>
          <TextBlock x:Name="Status" VerticalAlignment="Center" FontSize="12.5" Foreground="#FF8E949C" TextWrapping="Wrap" Margin="0,0,16,0"/>
        </DockPanel>
      </Border>
      <ScrollViewer VerticalScrollBarVisibility="Auto">
        <Grid Margin="34,28,34,24">
          <StackPanel x:Name="PGeneral">
            <TextBlock Text="General" Style="{StaticResource Heading}"/>
            <Grid Margin="0,0,0,22">
              <Grid.ColumnDefinitions><ColumnDefinition Width="*"/><ColumnDefinition Width="Auto"/></Grid.ColumnDefinitions>
              <StackPanel Margin="0,0,24,0"><TextBlock Text="Start SnapDrop with Windows" Style="{StaticResource Label}"/>
                <TextBlock Text="Runs quietly in the tray when you sign in." Style="{StaticResource Hint}"/></StackPanel>
              <CheckBox x:Name="SwStartup" Grid.Column="1" Style="{StaticResource Switch}"/>
            </Grid>
            <StackPanel Margin="0,0,0,22">
              <TextBlock Text="Save screenshots to" Style="{StaticResource Label}"/>
              <Grid Margin="0,8,0,0">
                <Grid.ColumnDefinitions><ColumnDefinition Width="*"/><ColumnDefinition Width="Auto"/><ColumnDefinition Width="Auto"/></Grid.ColumnDefinitions>
                <TextBox x:Name="TxtFolder" Style="{StaticResource Field}" IsReadOnly="True"/>
                <Button x:Name="BtnBrowse" Grid.Column="1" Content="Browse…" Style="{StaticResource Btn}" Margin="8,0,0,0"/>
                <Button x:Name="BtnOpenFolder" Grid.Column="2" Content="Open" Style="{StaticResource Btn}"/>
              </Grid>
            </StackPanel>
            <Grid Margin="0,0,0,22">
              <Grid.ColumnDefinitions><ColumnDefinition Width="*"/><ColumnDefinition Width="Auto"/></Grid.ColumnDefinitions>
              <StackPanel Margin="0,0,16,0"><TextBlock Text="Keep a copy in that folder" Style="{StaticResource Label}"/>
                <TextBlock Text="Smart: if you drag a screenshot into a folder, the desktop or an app, no extra copy is kept here. If you don't, it is saved here automatically." Style="{StaticResource Hint}"/></StackPanel>
              <StackPanel Grid.Column="1" Orientation="Horizontal" VerticalAlignment="Top">
                <RadioButton x:Name="PolSmart" GroupName="pol" Content="Smart" Style="{StaticResource ToolBtn}"/>
                <RadioButton x:Name="PolAlways" GroupName="pol" Content="Always" Style="{StaticResource ToolBtn}"/>
              </StackPanel>
            </Grid>
            <StackPanel Margin="0,0,0,22">
              <TextBlock Text="File name" Style="{StaticResource Label}"/>
              <TextBox x:Name="TxtPrefix" Style="{StaticResource Field}" Width="240" HorizontalAlignment="Left" Margin="0,8,0,0" MaxLength="40"/>
              <TextBlock x:Name="TxtExample" Style="{StaticResource Hint}"/>
            </StackPanel>
            <Grid Margin="0,0,0,22">
              <Grid.ColumnDefinitions><ColumnDefinition Width="*"/><ColumnDefinition Width="Auto"/></Grid.ColumnDefinitions>
              <StackPanel><TextBlock Text="Image format" Style="{StaticResource Label}"/>
                <TextBlock Text="PNG is lossless. JPEG makes smaller files." Style="{StaticResource Hint}"/></StackPanel>
              <StackPanel Grid.Column="1" Orientation="Horizontal">
                <RadioButton x:Name="FmtPng" GroupName="fmt" Content="PNG" Style="{StaticResource ToolBtn}"/>
                <RadioButton x:Name="FmtJpg" GroupName="fmt" Content="JPEG" Style="{StaticResource ToolBtn}"/>
              </StackPanel>
            </Grid>
            <Grid Margin="0,0,0,22">
              <Grid.ColumnDefinitions><ColumnDefinition Width="*"/><ColumnDefinition Width="Auto"/></Grid.ColumnDefinitions>
              <StackPanel Margin="0,0,24,0"><TextBlock Text="Copy every screenshot to the clipboard" Style="{StaticResource Label}"/>
                <TextBlock Text="Paste it straight into chat apps and documents with Ctrl+V." Style="{StaticResource Hint}"/></StackPanel>
              <CheckBox x:Name="SwClipboard" Grid.Column="1" Style="{StaticResource Switch}"/>
            </Grid>
            <Grid Margin="0,0,0,0">
              <Grid.ColumnDefinitions><ColumnDefinition Width="*"/><ColumnDefinition Width="Auto"/></Grid.ColumnDefinitions>
              <StackPanel><TextBlock Text="Capture delay" Style="{StaticResource Label}"/>
                <TextBlock Text="Wait before capturing, so you can open a menu first." Style="{StaticResource Hint}"/></StackPanel>
              <StackPanel Grid.Column="1" Orientation="Horizontal">
                <RadioButton x:Name="Tm0" GroupName="tm" Content="Off" Style="{StaticResource ToolBtn}"/>
                <RadioButton x:Name="Tm3" GroupName="tm" Content="3 s" Style="{StaticResource ToolBtn}"/>
                <RadioButton x:Name="Tm5" GroupName="tm" Content="5 s" Style="{StaticResource ToolBtn}"/>
                <RadioButton x:Name="Tm10" GroupName="tm" Content="10 s" Style="{StaticResource ToolBtn}"/>
              </StackPanel>
            </Grid>
          </StackPanel>
          <StackPanel x:Name="PShortcuts" Visibility="Collapsed">
            <TextBlock Text="Shortcuts" Style="{StaticResource Heading}"/>
            <TextBlock Text="Click a box and press the keys you want. Backspace clears it." Style="{StaticResource Hint}" Margin="0,-10,0,20"/>
            <Grid Margin="0,0,0,16">
              <Grid.ColumnDefinitions><ColumnDefinition Width="*"/><ColumnDefinition Width="200"/><ColumnDefinition Width="Auto"/></Grid.ColumnDefinitions>
              <TextBlock Text="Capture an area" Style="{StaticResource Label}" VerticalAlignment="Center"/>
              <TextBox x:Name="HkRegion" Grid.Column="1" Style="{StaticResource Field}"/>
              <Button x:Name="ClrRegion" Grid.Column="2" Content="Clear" Style="{StaticResource Btn}" Margin="8,0,0,0"/>
            </Grid>
            <Grid Margin="0,0,0,16">
              <Grid.ColumnDefinitions><ColumnDefinition Width="*"/><ColumnDefinition Width="200"/><ColumnDefinition Width="Auto"/></Grid.ColumnDefinitions>
              <TextBlock Text="Capture the full screen" Style="{StaticResource Label}" VerticalAlignment="Center"/>
              <TextBox x:Name="HkScreen" Grid.Column="1" Style="{StaticResource Field}"/>
              <Button x:Name="ClrScreen" Grid.Column="2" Content="Clear" Style="{StaticResource Btn}" Margin="8,0,0,0"/>
            </Grid>
            <Grid Margin="0,0,0,16">
              <Grid.ColumnDefinitions><ColumnDefinition Width="*"/><ColumnDefinition Width="200"/><ColumnDefinition Width="Auto"/></Grid.ColumnDefinitions>
              <TextBlock Text="Capture a window" Style="{StaticResource Label}" VerticalAlignment="Center"/>
              <TextBox x:Name="HkWindow" Grid.Column="1" Style="{StaticResource Field}"/>
              <Button x:Name="ClrWindow" Grid.Column="2" Content="Clear" Style="{StaticResource Btn}" Margin="8,0,0,0"/>
            </Grid>
            <Grid Margin="0,0,0,24">
              <Grid.ColumnDefinitions><ColumnDefinition Width="*"/><ColumnDefinition Width="200"/><ColumnDefinition Width="Auto"/></Grid.ColumnDefinitions>
              <TextBlock Text="Options bar" Style="{StaticResource Label}" VerticalAlignment="Center"/>
              <TextBox x:Name="HkBar" Grid.Column="1" Style="{StaticResource Field}"/>
              <Button x:Name="ClrBar" Grid.Column="2" Content="Clear" Style="{StaticResource Btn}" Margin="8,0,0,0"/>
            </Grid>
            <Grid Margin="0,0,0,22">
              <Grid.ColumnDefinitions><ColumnDefinition Width="*"/><ColumnDefinition Width="Auto"/></Grid.ColumnDefinitions>
              <StackPanel Margin="0,0,24,0"><TextBlock Text="Also use the Print Screen key" Style="{StaticResource Label}"/>
                <TextBlock Text="Print Screen starts an area capture." Style="{StaticResource Hint}"/></StackPanel>
              <CheckBox x:Name="SwPrint" Grid.Column="1" Style="{StaticResource Switch}"/>
            </Grid>
            <Button x:Name="BtnResetKeys" Content="Reset shortcuts to defaults" Style="{StaticResource Btn}" HorizontalAlignment="Left" Margin="0"/>
          </StackPanel>
          <StackPanel x:Name="PThumb" Visibility="Collapsed">
            <TextBlock Text="Thumbnail" Style="{StaticResource Heading}"/>
            <Grid Margin="0,0,0,22">
              <Grid.ColumnDefinitions><ColumnDefinition Width="*"/><ColumnDefinition Width="Auto"/></Grid.ColumnDefinitions>
              <StackPanel Margin="0,0,24,0"><TextBlock Text="Show a floating thumbnail after each capture" Style="{StaticResource Label}"/>
                <TextBlock Text="Drag it into any app, or click it to crop and mark it up." Style="{StaticResource Hint}"/></StackPanel>
              <CheckBox x:Name="SwThumb" Grid.Column="1" Style="{StaticResource Switch}"/>
            </Grid>
            <StackPanel Margin="0,0,0,22">
              <TextBlock Text="Where it appears" Style="{StaticResource Label}"/>
              <WrapPanel Margin="0,8,0,0">
                <RadioButton x:Name="CornTL" GroupName="corner" Content="Top left"     Style="{StaticResource ToolBtn}" Margin="0,0,4,4"/>
                <RadioButton x:Name="CornTR" GroupName="corner" Content="Top right"    Style="{StaticResource ToolBtn}" Margin="0,0,4,4"/>
                <RadioButton x:Name="CornBL" GroupName="corner" Content="Bottom left"  Style="{StaticResource ToolBtn}" Margin="0,0,4,4"/>
                <RadioButton x:Name="CornBR" GroupName="corner" Content="Bottom right" Style="{StaticResource ToolBtn}" Margin="0,0,4,4"/>
              </WrapPanel>
            </StackPanel>
            <Grid Margin="0,0,0,22">
              <Grid.ColumnDefinitions><ColumnDefinition Width="*"/><ColumnDefinition Width="Auto"/></Grid.ColumnDefinitions>
              <StackPanel><TextBlock Text="Stays on screen for" Style="{StaticResource Label}"/>
                <TextBlock Text="It pauses while your mouse is over it." Style="{StaticResource Hint}"/></StackPanel>
              <StackPanel Grid.Column="1" Orientation="Horizontal">
                <RadioButton x:Name="Dur5" GroupName="dur" Content="5 s" Style="{StaticResource ToolBtn}"/>
                <RadioButton x:Name="Dur9" GroupName="dur" Content="9 s" Style="{StaticResource ToolBtn}"/>
                <RadioButton x:Name="Dur15" GroupName="dur" Content="15 s" Style="{StaticResource ToolBtn}"/>
                <RadioButton x:Name="Dur30" GroupName="dur" Content="30 s" Style="{StaticResource ToolBtn}"/>
              </StackPanel>
            </Grid>
            <Grid Margin="0,0,0,0">
              <Grid.ColumnDefinitions><ColumnDefinition Width="*"/><ColumnDefinition Width="Auto"/></Grid.ColumnDefinitions>
              <StackPanel Margin="0,0,24,0"><TextBlock Text="Paste the file path when dropped on a terminal" Style="{StaticResource Label}"/>
                <TextBlock Text="Drop a thumbnail onto Windows Terminal (or the Claude Code CLI) and its file path is typed for you. Some terminals refuse file drops, so SnapDrop pastes the path instead." Style="{StaticResource Hint}"/></StackPanel>
              <CheckBox x:Name="SwTerminal" Grid.Column="1" Style="{StaticResource Switch}" VerticalAlignment="Top"/>
            </Grid>
          </StackPanel>
          <StackPanel x:Name="PAbout" Visibility="Collapsed">
            <TextBlock Text="About" Style="{StaticResource Heading}"/>
            <StackPanel Orientation="Horizontal" Margin="0,0,0,18">
              <Image x:Name="AboutLogo" Width="72" Height="72" RenderOptions.BitmapScalingMode="HighQuality"/>
              <StackPanel Margin="18,0,0,0" VerticalAlignment="Center">
                <TextBlock Text="SnapDrop" Foreground="White" FontSize="24" FontWeight="SemiBold"/>
                <TextBlock x:Name="AboutVersion" Foreground="#FF8E949C" FontSize="13"/>
              </StackPanel>
            </StackPanel>
            <TextBlock Foreground="#FFC9CDD3" FontSize="14" TextWrapping="Wrap" LineHeight="22"
                       Text="Screenshots you can drag anywhere. Capture, crop, then drag the floating thumbnail straight into a chat, document or folder, with no saving and no copy-paste. Free and open source under the MIT license."/>
            <WrapPanel Margin="0,22,0,0">
              <Button x:Name="BtnGithub" Content="GitHub page" Style="{StaticResource Btn}" Margin="0,0,6,6"/>
              <Button x:Name="BtnIssues" Content="Report an issue" Style="{StaticResource Btn}" Margin="0,0,6,6"/>
              <Button x:Name="BtnDataFolder" Content="Open settings folder" Style="{StaticResource Btn}" Margin="0,0,6,6"/>
            </WrapPanel>
            <Rectangle Height="1" Fill="#FF2C2E33" Margin="0,22,0,22"/>
            <TextBlock Text="Reset" Style="{StaticResource Label}"/>
            <TextBlock Text="Restore every setting to its default. Your screenshots are not touched." Style="{StaticResource Hint}"/>
            <Button x:Name="BtnResetAll" Content="Reset all settings" Style="{StaticResource Btn}" HorizontalAlignment="Left" Margin="0,12,0,0"/>
          </StackPanel>
        </Grid>
      </ScrollViewer>
    </DockPanel>
  </Grid>
</Window>
""";
    }
}
