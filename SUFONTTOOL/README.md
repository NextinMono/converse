# Sonic Unleashed Font Tool
A tool to convert Unleashed Font-Texture (FTE) and Font-Converse (FCO) files into XML files and vice versa.

## Warnings
- The current formatting and naming schemes are always subject to change.
- The new JSON files are yet to be tested properly, expect a small few to be incorrect.

## Usage
`SonicUnleashedFCOConv <Path to .fte/.fco/.xml>`\
You can also simply drag and drop supported file formats onto the executable.

## To Do
- (Ongoing) Update Translation JSONs for new system
- (Try) Add DDS Support

## Thanks
Big thank you to [TheExentist151](https://github.com/TheExentist151) for letting me recycle some of the code from [SonicColorsXTBConv](https://github.com/TheExentist151/SonicColorsXTBConv)<br>
Plus a HUGE thank you to [NextinHKRY](https://github.com/NextinMono) for all of your coding help!!

## Requirements
If the tool does nothing when used, you may need to install the [.NET Runtime](https://aka.ms/dotnet-core-applaunch?missing_runtime=true&arch=x64&rid=win10-x64&apphost_version=8.0) package that the tool needs to work

***

## Formats
### Font-Texture (Experimental)
FTE file formats contain data which help assign texture sprites to character slots to be used by the FCO in game.\
When converted to an XML you can view and edit the different fields to your liking.
```xml
<FTE>
  <Textures>        - A list of Usable Textures in the Scene Language archive 
    <Texture Name="mat_comon_x360_002" Size_X="512" Size_Y="512" />
    <Texture Name="mat_comon_x360_002" Size_X="512" Size_Y="512" />
    <Texture Name="fte_ConverseMain_000" Size_X="512" Size_Y="512" />
  </Textures>
  <!--ConverseID = Hex, Points = Px, Point1 = TopLeft, Point2 = BottomRight-->
  <Characters>      - Mappings of each Character within their Texture file
    <Character TextureIndex="0" ConverseID="00 00 00 64" Point1_X="0" Point1_Y="0" Point2_X="28" Point2_Y="28" />
    <Character TextureIndex="0" ConverseID="00 00 00 65" Point1_X="30" Point1_Y="0" Point2_X="58" Point2_Y="28" />
    <Character TextureIndex="0" ConverseID="00 00 00 66" Point1_X="60" Point1_Y="0" Point2_X="88" Point2_Y="28" />
    <Character TextureIndex="0" ConverseID="00 00 00 67" Point1_X="90" Point1_Y="0" Point2_X="118" Point2_Y="28" />
    ...         - There are a lot more + You can assign your own too as long as it's
                                         outside the used characters and within the overall character limit
  </Characters>
</FTE>
```

### Font-Converse
FCO file formats contain data such as text and cell properties to be used by the game when text needs to be called and displayed.\
When converted to an XML, you can view and edit the different fields such as:
```xml
<FCO Table="Languages/English/Retail/WorldMap">     - Table which define the Translation Table to be used
  <Groups>
    <Group Name="wm_hint_start">                    - Groups which hold Cells
      <Cell Name="smile" Alignment="0">             - Cells that contain Text Message and Format data
        <Message MessageData="I made you this World Map. I{NewLine}thought it might come in handy." />
        <ColourMain Start="0" End="59" Marker="2" Alpha="255" Red="255" Green="255" Blue="255" />
        <ColourSub1 Start="0" End="59" Marker="1" Alpha="0" Red="0" Green="0" Blue="28" />
        <ColourSub2 Start="0" End="59" Marker="0" Alpha="0" Red="0" Green="0" Blue="1" />
        <Highlight0 Start="0" End="14" Marker="2" Alpha="255" Red="255" Green="255" Blue="255" />
        <Highlight1 Start="15" End="24" Marker="2" Alpha="255" Red="252" Green="243" Blue="5" />
        <Highlight2 Start="25" End="25" Marker="2" Alpha="255" Red="204" Green="255" Blue="204" />
        <Highlight3 Start="26" End="59" Marker="2" Alpha="255" Red="255" Green="255" Blue="255" />
      </Cell>
      ...       - You can have any amount of Cells within a Group
    </Group>
    ...         - As well as Groups within a file
  </Groups>
</FCO>
```