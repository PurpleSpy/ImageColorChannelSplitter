# ImageColorChannelSplitter

this is used to split an images into various color channels,
at this time cmyk and hue staturation and brightness is still buggy

a few channels are labeled black or white in addition to the color, that would be the white balance for the channel

ChannelSplit 1.0.0.0
Useage (-options) arg1-argx
Args may be individual image files or directories, zipfiles if chosen or individual files will be placed in the same folder as original image
--- OPTIONS ---
-a Copies Alpha Channel
-A Creates a color overlay in the color chosen
-c Copies cyan Channel
-m Copies magenta Channel
-y Copies yellow Channel
-k Copies black Channel
-r Copies redblack Channel
-R Copies redwhite Channel
-g Copies greenblack Channel
-G Copies greenwhite Channel
-b Copies blueblack Channel
-B Copies bluewhite Channel
-u Copies hue Channel
-t Copies brightness Channel
-T Copies saturation Channel
-e Copies grayscale Channel
-f copy files rather than zip them up
-i invert image color

EX: ChannelSplit -AcR image.jpg "C:\imgdir" ....
