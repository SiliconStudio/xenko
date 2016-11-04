# the global way to work of this script is as follows:
#  some root directory is recursively walked, and open each .cs file in it.
#  each file is scanned line by line, building up sequentially a list of annotations everything a "[" line is encountered.
#  then the first class declaration met after is parsed for : Asset inheritance, and then if everything checks up,
#  the annotations bound to this class are filtered to look for "Display" type annotation only.
#  the display annotation line string, its file, and line in the file, are added to a dictionary.
#  after that, the line is parsed to extract the number in Display(100, "stuff"), so here 100 would be extracted.
#  then unicity over the whole database is checked, if collision issues are found, they are reported.
#  the report output respects the visual studio format https://msdn.microsoft.com/en-us/library/yxkt8b26.aspx

# besides. this same script can be used as a weight changer to shift and scale weights, from command line.
# example: "$script.exe xenko/ scale 100 shift 10"
#  that would multiply by 100 and shift up by 10, all values of Display annotation.
# in this mode, the script does not emit unicity error messages.

# a third mode can be used to extract the list of all detected weights, to generate documentation.
# example: "$script.exe xenko/ --dump"


import sys
import os
import re
import collections
import argparse

parser = argparse.ArgumentParser(description='script to scan xenko codebase and detects asset display weight annotation collisions, or rescale/shift weights, or just display them.')
parser.add_argument('rootdir',
                    help='root directory for source scanning')
parser.add_argument('--scale', type=float,
                    help='scale multiplier for weights')
parser.add_argument('--shift', type=int,
                    help='shift additionner for weights')
parser.add_argument('--dump', action='store_true',
                    help='shift additionner for weights')

args = parser.parse_args()

# adjust your root directory
rootdir = "c:/work/xenko/" if not args.rootdir else args.rootdir

print "display weight checker: scanning", rootdir

adjustmode = False
dumpmode = True if args.dump else False
scale = 1
shift = 0

if args.scale:
    adjustmode = True
    scale = args.scale
    
if args.shift:
    adjustmode = True
    shift = args.shift

if adjustmode:
    print "going to modify weights with scale", scale, "and shift", shift
    dumpmode = False # priority is adjust.

# store display annotations (and their location in the .cs file)
allDisplayAnnots = []

def filterForDisplayAnnot(annots):
    '''pass a list of string annotations and get THE ONE display annotation string if exists in the list. otherwise None'''
    f = [x for x in annots if re.search("\[Display\(\d+", x[0])]
    if len(f) > 0:
        return f[0]
    else:
        return None

def checkline(l, fn, annots):
    '''pass one C# source code line, and let the function fill in a global database, if the line is an Asset class declaration'''
    global allDisplayAnnots
    lt = l.strip()
    if re.search("class.*\:.*Asset", lt):   # check a pattern for inheritance declaration "***class *** : Asset"
        disp = filterForDisplayAnnot(annots)
        if disp is not None:
            allDisplayAnnots.append((fn, disp))
    return

def appendAnnotation(a, alist, linecnt):
    '''checks if a line is an annotation line, and add it to the current list of annotations'''
    tr = a.strip()
    if tr.startswith("["):
        alist.append((tr, linecnt))
    elif len(tr) == 0 or tr.startswith("//"):
        pass # do nothing in case of empty lines, or comment lines. they don't interrupt the annotation list.
    else:
        alist = []

def checkfile(srcfile):
    '''check one cs source file, for all lines, parse for class decl'''
    #print "now checking file", srcfile.name, "..."
    alist = []
    linecnt = 0
    for lines in fin:
        checkline(lines, os.path.abspath(srcfile.name), alist)
        appendAnnotation(lines, alist, linecnt)
        linecnt = linecnt + 1

for root, subFolders, files in os.walk(rootdir):
    for src in [x for x in files if x.endswith(".cs")]:
        with open(os.path.join(root, src), 'r') as fin:
            checkfile(fin)

# allDisplayAnnots format is:
# a list of pairs. with each pair being:
#  [0] is filename
#  [1] is a tuple. with:
#    .[0] the raw line content of the annotation
#    .[1] the line number in the file

# collapse duplicated results (for some reason we have duplicates).
# by transferring the database to a dictionary form, the same keys are going to get "folded" to one.
# the key is a tuple consisting of the filename and the line number. as the value, we extract the sorting weight of the display annotation line.
ada = {}
for a in allDisplayAnnots:
    ada[(a[0], a[1][1])] = int(re.search("\d+", a[1][0]).group(0))

# secondarily, for the "adjustmode", let's also don't forget the original annotation line string:
ral = {}
for a in allDisplayAnnots:
    ral[(a[0], a[1][1])] = a[1][0]  # raw annotation line

# linearize it, to be accessible by indexes, since we are going to have two side-by-side collections,
# the weights and ada2, so we neet the indices to match:
ada2 = []
for k, v in ada.iteritems():
    ada2.append((k, v)) 

# create the weights list as a standalone list of ints:
weights = []
for k, v in ada2:
    weights.append(v)

if adjustmode:
    # need to copy the whole source file in memory, change the desired zone and rewrite the whole thing.
    for record in ada2:
        origweight = ada[record[0]]  # original weight
        newweight = int(origweight * scale + shift)
        linenum = int(record[0][1])
        # set the result in the file, by reading all first:
        with open(record[0][0]) as f:
            content = f.readlines()
        origline = content[linenum]  # use the original line instead of record[0] because the original is non-trimmed
        # modify the line
        newline = origline.partition("(")[0] + "("
        newline = newline + str(newweight) 
        newline = newline + "," + origline.partition(",")[2]
        # modify the line:
        content[linenum] = newline
        # and write all back:
        with open(record[0][0], 'wb') as f:
            print "modifying file", f.name, "(", linenum, "). old:", origweight, "to:", newweight
            for l in content:
                f.write(l)
    print "modified", len(ada2), "records"
            
elif dumpmode:  # documentation mode
    for record in ada2:
        print "weight", record[1], "at file", record[0]
    print len(ada2), "results"
else:  # error report mode:
    # detect redundancy
    if len(weights) > len(set(weights)):
        dups = [item for item, count in collections.Counter(weights).items() if count > 1]
        #print dups
        for d in dups:
            for i in range(0, len(weights)):
                if weights[i] == d:
                    print ada2[i][0][0] + "(" + str(ada2[i][0][1]) + ") : error : duplicated display weight " + str(d) 
    else:
        print "display weight check: no sorting weights collision (all OK)"
