# Git LFS

## Rationale

In version 1.6, we moved away from submodules and switched to Git LFS to simplify our workflow for large files and place them wherever we want.

## Requirements

You need both Git (if possible version >=2.8, but >=1.8.2 is technically enough but slower) and Git LFS (1.2.0).
```
$ git version
git version 2.8.1.windows.1
$ git lfs version
git-lfs/1.2.0 (GitHub; windows amd64; go 1.6.1; git 386c5d8)
```

Download Git (2.9): https://git-scm.com/download/win
Download Git LFS: https://git-lfs.github.com/

## Cloning

If you want to start from a clean repository, use `git lfs clone git@git.xenko.com:xenko/Xenko-Runtime.git` rather than `git clone`, since it is much faster.

## Updating an existing repository to a version using LFS

> **IMPORTANT**
>
> If you switch to LFS for the first time, this might take a while.
> Also, please backup your submodules if you had any non-committed changes in them, and make sure your `git status` is empty.

Fetch the latest versions:
```
git fetch --all
```

Remove submodules:
```
git submodule deinit .
```

Checkout the latest master **(SLOW)**:
```
git checkout -B master origin/master
```

Checkout latest master **(FAST)** (git 2.8+):
```
git -c filter.lfs.smudge= -c filter.lfs.required=false checkout -b master origin/master -f
git lfs pull
```

## How to update my existing branches to Git LFS?

> **IMPORTANT** 
>
> Unfortunately, Git will get confused when going back and forth between **pre-LFS** and **post-LFS** states, due to the fact that we kept the same directory structure. As a result, int the update process you will have to always stay on **post-LFS** branch.
> 
> **Please avoid going back to non-LFS versions of Xenko after you are done. Repeating this process for every branch should work out fine!**
>
> If your branch is a single commit, feel free to cherry-pick on top of master instead.

First, get the latest master with a new branch (which is already converted to LFS):

```
git checkout origin/master -b <yourbranch>-lfs
```

Merge your branch against it:
```
git merge <yourbranch>
```

You will have two types of conflicts:
* Normal conflicts: Merge them as usual
* Submodules conflicts (if you had any changes in them): Please **do not merge these**

To resolve submodule conflicts (since the old submodules are now plain folders):
* Use `git rm --cached <submodule_path>`
* Replace/merge files with the new version manually (keeping a separate checkout is handy to copy them)
* To do this merge, we recommend to checkout the submodule, do the merge with your branch and master, then copy files over to your new Xenko repo (deleting submodule files beforehand, if you had any deleted files)
* Take care not to copy over the .git folder or file if you copy from a separate submodule checkout

If you had conflicts, double-checking that the submodule's folder is properly merged via `git status` and commit:

```
git commit -m "Merged master (now using Git LFS)"
```

Finally, move your old branch here:

```
git checkout <yourbranch>-lfs -b <yourbranch> -f
```
