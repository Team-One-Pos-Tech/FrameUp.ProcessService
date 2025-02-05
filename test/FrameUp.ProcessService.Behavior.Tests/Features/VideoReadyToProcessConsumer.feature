Feature: Process a video into thumbnails

    Scenario: A Valid bucket with a video is submited, should succeed
        Given a valid bucket for order id '919806ea-f94d-401b-a1f6-9a15291c4125' with a video stored
        When a ReadyToProcessVideo event is risen for order id '919806ea-f94d-401b-a1f6-9a15291c4125'
        Then it should process the video successfully for order id '919806ea-f94d-401b-a1f6-9a15291c4125' with name 'packages/2025-02-04 00:00:00Z_snapshots.zip'