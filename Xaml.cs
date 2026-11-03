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
    }
}
