# Gignore

Generates a .gitignore based on a supplied list of template names.\
The programme calls TopTal's API under the hood, which in turn uses GitHub's .gitignore templates.

## Setup

Execute the following command to build the programme:

```
dotnet publish src -o <output directory>
```

Then add that directory to your PATH.

## Usage

To compile a .gitignore in your current directory, execute the following command,
adding as many template names as you wish.
```
gignore <template names...>
```

To have the full list of available template names written to a file named `list.txt`,
execute instead
```
gignore list
```

To search for template names that contain particular search terms and have any matches
printed to standard output, execute
```
gignore search <search terms>
```


