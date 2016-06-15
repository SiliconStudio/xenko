LOCAL_PATH:= $(call my-dir)

include $(CLEAR_VARS)

LOCAL_MODULE := NativeLibrary
LOCAL_C_INCLUDES := 


LOCAL_ARM_MODE := arm
LOCAL_CFLAGS := $(LOCAL_C_INCLUDES:%=-I%) -O3 -DANDROID_NDK
LOCAL_LDLIBS := -L$(SYSROOT)/usr/lib -ldl -llog

LOCAL_SRC_FILES := ../NativeLibrary.cpp

include $(BUILD_SHARED_LIBRARY)