FetchWallpaper
==============
Because I was tired of my current wallpaper and losing a great amount of time searching for a new, I created this program to help me a bit. The goal is to have a program that runs in the background and that searches every hour for a new one. 

Usage
-----
This program is a console application. Creating a UI for this is a bit overkill. 
	
	FetchWallpaper.exe [OPTIONS] <random|member>
	
Options

	-d, --delay=VALUE          The amount of time in minutes the program waits to fetch a new wallpaper
	-u, --user=VALUE           The id of the user you want the favourites from. Only usable in MEMBER mode.
	-f, --folder=VALUE         The id of the user folder. Only usable in MEMBER mode. This is optional.
	-c                         The id's of the categories you want in RANDOM mode.
	-h, --help                 Show this message and exit
	
This application has 2 modes. You can run in *random* mode and in *member*. Default you are always running in random mode and you can define categories to filter. In member mode, you can define a user id (from the Wallpaperup site) and an optional folder id (A categorized favourite from the given user) to select a random wallpaper.

To run this program at startup. Just create a shortcut of this program and place this in your startup folder (start > programs > startup). Then in the properties of this shortcut, set the target to something like this

	C:\<change this to your path>\FetchWallpaper.exe -u 1 -f 5 -d 30 member
	
So next time you run this program, you will get a wallpaper from user id 1, favourites category id 5 every 30 minutes.

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

Examples
--------

When you don't give any arguments, it will just pick a random featured wallpaper.

	FetchWallpaper.exe
	
This example will pick a random wallpaper from the car (id 39) or trains (id 106) category.
	
	FetchWallpaper.exe -c 39 106 random
	
This example will pick a wallpaper from the favourites of user 1.
	
	FetchWallpaper.exe -u 1 member
	
This example will pick a wallpaper from the favourites category animals (id 5) of user 1	

	FetchWallpaper.exe -u 1 -f 5 -d 30 member

Thanks
------
A special thanks to www.wallpaperup.com for the great API that I can use.