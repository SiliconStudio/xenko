# Git LFS

## Rationale

We recently ditched submodules and switched to Git LFS to simplify our workflow for large files, and be able to place them wherever we want.

## Requirements

You need both git (if possible version >=2.8, but >=1.8.2 is technically enough but slower) and git lfs (1.2.0).
```
$ git version
git version 2.8.1.windows.1
$ git lfs version
git-lfs/1.2.0 (GitHub; windows amd64; go 1.6.1; git 386c5d8)
```

Download git (2.9): https://git-scm.com/download/win
Download git lfs: https://git-lfs.github.com/

## Cloning from scratch

If you want to start from a clean repository, please use `git lfs clone git@github.com:SiliconStudio/xenko.git` rather than `git clone`, as it is much faster (it batches everything).

## Updating an existing repository to a version using LFS

> **IMPORTANT**
>
> If your first time to switch to LFS, it might take a while.
> Also, please backup your submodules if you had any non-committed changes in them, and please make sure your `git status` is empty.

Fetch latest versions (if you didn't do it before):
```
git fetch --all
```

Remove submodules
```
git submodule deinit .
```

Checkout latest master **SLOW MODE**:
```
git checkout -B master origin/master
```

Checkout latest master **FASTER MODE** (git 2.8+):
```
git -c filter.lfs.smudge= -c filter.lfs.required=false checkout -b master origin/master -f
git lfs pull
```

## How to update my existing branches to git lfs?

> **Important notice** 
>
> Unfortunately, git is confused when going back and forth between **pre-lfs** and **post-lfs** state due to the fact we kept the same directory structure. As a result, this update process is done so that you always stay on **post-lfs** side!
> 
> **Please avoid going back to non-LFS version of Xenko after you are done! Just repeating this process for every branch should work out fine!**
>
> Also, if your branch is a single commit, feel free to cherry-pick on top of master instead.

First, get the latest master with a new branch (which is already converted to lfs):

```
git checkout origin/master -b <yourbranch>-lfs
```

Then, we'll merge your branch against it:
```
git merge <yourbranch>
```

You will have two types of conflicts:
* normal conflicts, merge them as usual
* submodules conflicts (if you had any changes in them), please **do not merge those**

Concerning submodule conflicts, since both the old submodule now became a folder, here is the proper way to resolve it:
* Use `git rm --cached <submodule_path>`
* then replace/merge files with new version manually (probably keeping a separate checkout would be handy to copy them)
* to do that merge, it is recommended to checkout the submodule, do the merge with your branch and master, then copy files over to your new xenko repo (delete submodule files before if you had any deleted files)
* take care not to copy over the .git folder or file if you copy from a separate submodule checkout

If you had conflicts, after double-checking submodules folder is properly merged in status, you can commit:

```
git commit -m "Merged master (now using Git LFS)"
```

Finally, move your old branch here:

```
git checkout <yourbranch>-lfs -b <yourbranch> -f
```
