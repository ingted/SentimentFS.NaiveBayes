namespace SentimentFS.NaiveBayes.Tests

open Expecto
open SentimentFS.Core.Caching
open SentimentFS.NaiveBayes.Training
open SentimentFS.NaiveBayes.Dto

module Trainer =

    [<Tests>]
    let tests =
        testList "Trainer" [
            testList "empty" [
                testCase "test get empty trainer function" <| fun _ ->
                    let subject = Trainer.init<int>(None) |> Trainer.get
                    Expect.isNone subject "should be None"
            ]
            testList "parseTokens" [
               testCase "parse" <| fun _ ->
                  let tokens = "cute dog" |> Trainer.parseTokens(Config.Default())
                  Expect.equal (tokens) (["cute"; "dog"]) "tokens should has two keys"
            ]
            testList "incrementTrainings" [
                test "increment training" {
                    let subject = State.empty() |> Trainer.incrementTrainings
                    Expect.equal (subject.trainings) 1 "should equal 1"
                }
            ]
            testList "categorize" [
               testCase "when category no exist in state" <| fun _ ->
                   let subject = (Config.Default(), State.empty()) |> Trainer.categorize(2, ["cute"; "dog"])
                   Expect.isTrue (subject.categories.ContainsKey(2)) "should contain 2"
                   Expect.equal ((subject.categories.[2]).trainings) 1 "trainings should equal 1"
                   Expect.equal ((subject.categories.[2]).tokens) ([("cute", 1); ("dog", 1)] |> Map.ofList) "trainings should equal 1"
               testCase "when category exist in state" <| fun _ ->
                   let state = {categories = ([(2, { trainings = 1; tokens = ([("test", 2)] |> Map.ofList) })] |> Map.ofList); trainings = 1 }
                   Expect.isTrue (state.categories.ContainsKey(2)) "should contain 2"
                   let subject = (Config.Default(), state) |> Trainer.categorize(2, ["cute"; "dog"])
                   Expect.isTrue (subject.categories.ContainsKey(2)) "should contain 2"
                   Expect.equal ((subject.categories.[2]).trainings) 2 "trainings should equal 2"
                   Expect.equal ((subject.categories.[2]).tokens) ([("test", 1); ("cute", 1); ("dog", 1)] |> Map.ofList) ""
            ]
            testList "train" [
                testCase "test train function (one training)" <| fun _ ->
                    let subject = Trainer.init<int>(None)
                                    |> Trainer.train({ value = "test"; category = 2; weight = None })
                                    |> Trainer.get
                    Expect.isSome subject "should be some"

                    Expect.equal (subject.Value.trainings) (1) "trainings quantity should equal 1"

                    let categoryOpt = subject.Value.categories.TryFind(2)
                    Expect.isSome categoryOpt "category of 2 should be some"
                    Expect.equal (categoryOpt.Value) ({ trainings = 1; tokens = ([("test", 1)] |> Map.ofList) }) "should"
                testCase "test train function (two training, one category)" <| fun _ ->
                    let subject = Trainer.init<int>(None)
                                    |> Trainer.train({ value = "test"; category = 2; weight = None })
                                    |> Trainer.train({ value = "test2"; category = 2; weight = None })
                                    |> Trainer.get
                    Expect.isSome subject "should be some"

                    Expect.equal (subject.Value.trainings) (2) "trainings quantity should equal 2"

                    let categoryOpt = subject.Value.categories.TryFind(2)
                    Expect.isSome categoryOpt "category of 2 should be some"
                    Expect.equal (categoryOpt.Value) ({ trainings = 2; tokens = ([("test", 1); ("test2", 1)] |> Map.ofList) }) "should"
                testCase "test train function (two training, two category)" <| fun _ ->
                    let subject = Trainer.init<int>(None)
                                    |> Trainer.train({ value = "test"; category = 2; weight = None })
                                    |> Trainer.train({ value = "test2"; category = 1; weight = None })
                                    |> Trainer.get
                    Expect.isSome subject "should be some"

                    Expect.equal (subject.Value.trainings) (2) "trainings quantity should equal 2"

                    let categoryOpt = subject.Value.categories.TryFind(2)
                    Expect.isSome categoryOpt "category of 2 should be some"
                    Expect.equal (categoryOpt.Value) ({ trainings = 1; tokens = ([("test", 1)] |> Map.ofList) }) "should"

                    let categoryOpt2 = subject.Value.categories.TryFind(1)
                    Expect.isSome categoryOpt2 "category of 2 should be some"
                    Expect.equal (categoryOpt2.Value) ({ trainings = 1; tokens = ([("test2", 1)] |> Map.ofList) }) "should"
            ]
        ]
