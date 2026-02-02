#!/usr/bin/env dotnet fsi

// Test that toolMap operation no longer exists (Task 10.8)
// This script SHOULD FAIL to compile

#r "src/FsAgent/bin/Debug/netstandard2.0/FsAgent.dll"

open FsAgent
open FsAgent.Agents

// Attempting to use toolMap (should fail)
let testAgent = agent {
    name "test"
    description "This should fail"
}

// Try calling toolMap as a method (this is what the DSL operation would translate to)
// This should produce: "The field, constructor or member 'toolMap' is not defined"
let _ = AgentBuilder().toolMap(testAgent, [("write", true)])

printfn "If you see this, toolMap still exists (FAIL)"
