<html>
    <h1 align='center'>
        Parentiphy
    </h1>
    <p align='center'>
        Remove redundant folders from a given path.
        <br><br>
        <a href="https://github.com/yumiris/Parentiphy/releases/latest">
            Download
        </a>
    </p>
</html>

## Introduction

This oddly-named program will remove those irritating folders that appear after you extract an archive:

![Screenshot of the desired folder layout.](https://user-images.githubusercontent.com/10241434/131263362-08c8849a-faa7-45e0-8ed5-1aa36f8f82a8.png)

It does it gracefully and elegantly, and accomplishes the same result as:

1.  WinRAR's "Remove redundant folders from extraction path." option; or
2.  Ark's (through Dolphin) "Extract here, Autodetect Subfolder" procedure.

## Usage

The latest rudimentary release can be found [here](https://github.com/yumiris/Parentiphy/releases/latest)!

The usage is as wacky as the name of this program, let alone the problem it solves:

```sh
parent --for "~/directory" # removes redundant folder in ~/directory
parent --all "~/directory" # removes redundant folders in ~/directory's top level folders
```
```sh
7z x "~/data.7z" -o"~/data" && parent --for "~/data"
```

## Mechanism

The program will handle a given directory ONLY if it fulfils the following requirements:

1.  It has ONE top-level directory; AND
2.  It has ZERO top-level files.

The inferred top-level directory will thus be deemed as the redundant one. All of its files and directories will be moved to the base directory.
