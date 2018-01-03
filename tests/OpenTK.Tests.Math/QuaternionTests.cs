﻿using Xunit;
using OpenTK.Tests.Math.Helpers;
using OpenTK.Tests.Math.DataProviders;

namespace OpenTK.Tests.Math
{
	public class Quaternion_Tests
	{
		/// <summary>
		/// Contains all tests which cover the ctor of Quaternion.
		/// </summary>
		public class Constructor_Tests
		{
			/// <summary>
			/// Checks if a single given value (either pitch, yaw or roll) get converted into correct x,y,z value of quaternion.
			/// </summary>
			/// <param name="eulerValues">euler angle values</param>
			/// <param name="expectedResult">expected xyz component of quaternion</param>
			/// <param name="testName">Taken from nUnit test data. Don't know how to name data driven tests for xUnit which actually works.</param>
			[Theory]
			[MemberData(nameof(QuaternionTestDataGenerator.SingleAxisTestCases), MemberType = typeof(QuaternionTestDataGenerator))]
			public void SingleAxisAsEulerAnglesInFloatsIsConvertedToCorrectValueInQuaternion
			(Vector3 eulerValues, Vector3 expectedResult, string testName)
			{
				//Arrange + Act: Create Quaternion with "pitch/yaw/roll"
				var cut = new Quaternion(eulerValues.X, eulerValues.Y, eulerValues.Z);

				//Assert: Use helper, to check if part of the two correct axis is zero. I just want check the direction
				Vector3 resultXYZ = cut.Xyz;

				Assert.True(QuaternionTestHelper.VerifyEqualSingleDirection(resultXYZ, expectedResult));
			}

			/// <summary>
			/// Checks if a single given value (either pitch, yaw or roll) get converted into correct x,y,z value of quaternion.
			/// </summary>
			/// <param name="eulerValues">euler angle values</param>
			/// <param name="expectedResult">expected xyz component of quaternion</param>
			/// <param name="testName">Taken from nUnit test data. Don't know how to name data driven tests for xUnit which actually works.</param>
			[Theory]
			[MemberData(nameof(QuaternionTestDataGenerator.SingleAxisTestCases), MemberType = typeof(QuaternionTestDataGenerator))]
			public void SingleAxisAsEulerAnglesInVector3IsConvertedToCorrectValueInQuaternion
			(Vector3 eulerValues, Vector3 expectedResult, string testName)
			{
				//Arrange + Act: Create Quaternion with "pitch/yaw/roll"
				var cut = new Quaternion(eulerValues);

				//Assert: Use helper, to check if part of the two correct axis is zero. I just want check the direction
				Vector3 resultXYZ = cut.Xyz;

				Assert.True(QuaternionTestHelper.VerifyEqualSingleDirection(resultXYZ, expectedResult));
			}

			//TODO: Make also checks with ccw rotation angle and correct applied rotation order of multiple axis
		}

		/// <summary>
		/// Contains all tests for FromEulerAngles
		/// </summary>
		public class FromEulerAngles_Tests
		{
			/// <summary>
			/// Checks if a single given value (either pitch, yaw or roll) get converted into correct x,y,z value of quaternion.
			/// </summary>
			/// <param name="eulerValues">euler angle values</param>
			/// <param name="expectedResult">expected xyz component of quaternion</param>
			/// <param name="testName">Taken from nUnit test data. Don't know how to name data driven tests for xUnit which actually works.</param>
			[Theory]
			[MemberData(nameof(QuaternionTestDataGenerator.SingleAxisTestCases), MemberType = typeof(QuaternionTestDataGenerator))]
			public void SingleAxisAsEulerAnglesInFloatsIsConvertedToCorrectValueInQuaternion
			(Vector3 eulerValues, Vector3 expectedResult, string testName)
			{
				//Arrange + Act: Create Quaternion with "pitch/yaw/roll"
				var cut = Quaternion.FromEulerAngles(eulerValues.X, eulerValues.Y, eulerValues.Z);

				//Assert: Use helper, to check if part of the two correct axis is zero. I just want check the direction
				Vector3 resultXYZ = cut.Xyz;

				Assert.True(QuaternionTestHelper.VerifyEqualSingleDirection(resultXYZ, expectedResult));
			}

			/// <summary>
			/// Checks if a single given value (either pitch, yaw or roll) get converted into correct x,y,z value of quaternion.
			/// </summary>
			/// <param name="eulerValues">euler angle values</param>
			/// <param name="expectedResult">expected xyz component of quaternion</param>
			/// <param name="testName">Taken from nUnit test data. Don't know how to name data driven tests for xUnit which actually works.</param>
			[Theory]
			[MemberData(nameof(QuaternionTestDataGenerator.SingleAxisTestCases), MemberType = typeof(QuaternionTestDataGenerator))]
			public void SingleAxisAsEulerAnglesInVector3IsConvertedToCorrectValueInQuaternion
			(Vector3 eulerValues, Vector3 expectedResult, string testName)
			{
				//Arrange + Act: Create Quaternion with "pitch/yaw/roll"
				var cut = Quaternion.FromEulerAngles(eulerValues);

				//Assert: Use helper, to check if part of the two correct axis is zero. I just want check the direction
				Vector3 resultXYZ = cut.Xyz;

				Assert.True(QuaternionTestHelper.VerifyEqualSingleDirection(resultXYZ, expectedResult));
			}

			/// <summary>
			/// Checks if a single given value (either pitch, yaw or roll) get converted into correct x,y,z value of quaternion.
			/// </summary>
			/// <param name="eulerValues">euler angle values</param>
			/// <param name="expectedResult">expected xyz component of quaternion</param>
			/// <param name="testName">Taken from nUnit test data. Don't know how to name data driven tests for xUnit which actually works.</param>
			[Theory]
			[MemberData(nameof(QuaternionTestDataGenerator.SingleAxisTestCases), MemberType = typeof(QuaternionTestDataGenerator))]
			public void SingleAxisAsEulerAnglesInVector3IsConvertedToCorrectValueInQuaternionAsOutParam
			(Vector3 eulerValues, Vector3 expectedResult, string testName)
			{
				//Arrange + Act: Create Quaternion with "pitch/yaw/roll"
				var cut = Quaternion.Identity;
				Quaternion.FromEulerAngles(ref eulerValues, out cut);

				//Assert: Use helper, to check if part of the two correct axis is zero. I just want check the direction
				Vector3 resultXYZ = cut.Xyz;

				Assert.True(QuaternionTestHelper.VerifyEqualSingleDirection(resultXYZ, expectedResult));
			}

			//TODO: Make also checks with ccw rotation angle and correct applied rotation order of multiple axis
		}


		/// <summary>
		/// Contains all test for ToAxisAngle
		/// </summary>
		public class ToAxisAngle
		{
			/// <summary>
			/// Check if a quaternion returns a a rotation about the correct coordinate axis
			/// </summary>
			/// <param name="cut">Prepared Quaternion</param>
			/// <param name="expectedResult">Expected result.</param>
			/// <param name="testName">Taken from nUnit test data. Don't know how to name data driven tests for xUnit which actually works.</param>
			[Theory]
			[MemberData(nameof(QuaternionTestDataGenerator.ToAxisAngleTestCases), MemberType = typeof(QuaternionTestDataGenerator))]
			public void PreparedSingleRotationAxisQuaternionConvertsToCorrectAxisAngleRepresentation(Quaternion cut, Vector3 expectedResult, string testName)
			{
				//Arrange + Act: Create Quaternion with rotation about X/Y/Z axis
				Vector3 resultXYZ;
				float dontCare;
				cut.ToAxisAngle(out resultXYZ, out dontCare);

				//Assert: Use helper, to check if part of the two correct axis is zero. I just want check the direction
				Assert.True(QuaternionTestHelper.VerifyEqualSingleDirection(resultXYZ, expectedResult));
			}

			//TODO: Make also checks with ccw rotation angle and correct applied rotation order of multiple axis
		}



	}
}

