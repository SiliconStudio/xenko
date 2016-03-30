//
//  ViewController.h
//  Xenko iOS Relay
//
//  Created by Virgile Bello on 2016/01/08.
//  Copyright © 2016年 Silicon Studio. All rights reserved.
//

#import <Cocoa/Cocoa.h>

@interface ViewController : NSViewController
{
    BOOL mRunning;
    pid_t mPid;
    NSString* mBundleFolder;
    NSTask* mTask;
    NSPipe* mPipe;
}

@property (weak) IBOutlet NSTextField *Address;
@property (weak) IBOutlet NSButton *StartStopButton;
@property (unsafe_unretained) IBOutlet NSTextView *LogView;

//http://www.raywenderlich.com/36537/nstask-tutorial

@end

