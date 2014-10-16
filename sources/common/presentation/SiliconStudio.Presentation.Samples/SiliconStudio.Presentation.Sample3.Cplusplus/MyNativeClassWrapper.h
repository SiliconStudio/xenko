#pragma once
#include "MyNativeClass.h"
#include "msclr\marshal_cppstd.h"

using namespace System;

// A wrapper for our very basic c++ class 

namespace SiliconStudio
{
namespace Presentation
{
namespace Sample3
{
namespace Cplusplus
{
	public ref class MyNativeClassWrapper
	{
	public:
		static MyNativeClassWrapper^ CreateInstance()
		{
			MyNativeClass *native = new MyNativeClass();
			return gcnew MyNativeClassWrapper(native);
		}

		property int IntValue { int get() { return native->IntValue; } void set(int value) { native->IntValue = value; } }

		property double DoubleValue { double get() { return native->DoubleValue; } void set(double value) { native->DoubleValue = value; } }

		property String^ StringValue { String^ get() { return msclr::interop::marshal_as<String^>(native->StringValue); } void set(String^ value) { native->StringValue = msclr::interop::marshal_as<std::string>(value); } }

		String^ PrintNativeValues()
		{
			String^ result = gcnew String("Values from the c++ object:");
			result += "\r\nIntValue: ";
			result += native->IntValue;
			result += "\r\nDoubleValue: ";
			result += native->DoubleValue;
			result += "\r\nStringValue: ";
			result +=  msclr::interop::marshal_as<String^>(native->StringValue);
			return result;
		}

	private:
		MyNativeClassWrapper(MyNativeClass *native)
		{
			this->native = native;
		}

		MyNativeClass *native;
	};
}
}
}
}

