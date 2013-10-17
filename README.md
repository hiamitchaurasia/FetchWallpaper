FetchWallpaper
==============
Because I was tired of my current wallpaper and losing a great amount of time searching for a new, I created this program to help me a bit. The goal is to have a program that runs in the background and that searches every hour for a new one. 

Usage
-----
This program is a console application. Creating a UI for this is a bit overkill. 
	
	FetchWallpaper.exe <categories>

You can give as arguments, the id's of the subcategories you want to have as background. If this isn't specified, it will just take a random wallpaper.

To run this program at startup. Just create a shortcut of this program and place this in your startup folder (start > programs > startup). Then in the properties of this shortcut, set the target to something like this

	C:\<change this to your path>\FetchWallpaper.exe 39
	
So next time you run this program, you will get a random **car** wallpaper, because 39 is the category id for cars.

This program also creates a log file. When you do not get any wallpapers, check this file or contact me if you have any questions.
	
Categories
----------
3D: 35
Abstract: 2
Aircrafts: 38
Animals: 3
Anime: 12
Architecture: 68
Baby Animals: 70
Beaches: 31
Birds: 71
Birthday: 65
Cars: 39
Cartoons: 116
Cats: 72
Children: 109
hristmas: 62
Cities: 69
Classic Games: 115
Comics: 117
Concept: 84
Dark: 111
Deserts: 43
Dogs: 73
Drinks: 92
Drops: 44
Easter: 63
Entertainment: 93
Fantasy: 112
Fishes: 74
Flags: 101
Flowers: 45
Food: 91
Fractal: 36
Fruits: 46
Halloween: 64
Holidays: 33
Humor: 113
Insects: 75
Lakes: 95
Landscapes: 47
Leaves: 97
Motorcycles: 40
Mountains: 49
Movies: 7
Music: 110
Nature: 8
New Year: 66
Other: 78
Other Abstract: 87
Other Animals: 77
Other Entertainment: 118
Other Females: 107
Other Holidays: 85
Other Males: 108
Other Nature: 82
Other People: 119
Other Vehicles: 88
Other World: 89
People: 5
Plants: 50
Quotes: 100
Reptiles: 76
Rivers: 51
Roads: 102
Sci-Fi: 114
Sea Ocean: 98
Sky: 53
Space: 54
Sports: 16
Sunrise Sunset: 96
Technology: 6
Texts: 99
Texture: 37
Trains: 106
Trees: 80
Trucks: 105
TV Series: 9
Valentine's Day: 67
Vector: 79
Vehicles: 32
Videogames: 34
VIP Females: 41
VIP Males: 42
Watercrafts: 104
Waterfalls: 81
World: 18

Thanks
------
A special thanks to www.wallpaperup.com for the great API that I can use.