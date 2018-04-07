#!/usr/bin/env python
# encoding: utf-8
import sys
import tweepy #https://github.com/tweepy/tweepy
import csv

#Twitter API credentials
consumer_key = "s2eARBY3F1huzPLxa9Yqrn9Hk"
consumer_secret = "PBVAq0Oto8ZpyK6RiXjjntgIQfdiaxlYMLPSiaY6AiNyzAtDNz"
access_key = "55485068-e0XXf2GVWVDNW7mLX47YfaFZvtzcLmaagt8CP3xwd"
access_secret = "PqjLHiQY1naSyLURWGohuCltjUAwE5UPeWoS1docaxYN2"


def get_all_tweets(screen_name):
	#Twitter only allows access to a users most recent 3240 tweets with this method
	
	#authorize twitter, initialize tweepy
	auth = tweepy.OAuthHandler(consumer_key, consumer_secret)
	auth.set_access_token(access_key, access_secret)
	api = tweepy.API(auth)
	
	#initialize a list to hold all the tweepy Tweets
	alltweets = []	
	
	#make initial request for most recent tweets (200 is the maximum allowed count)
	new_tweets = api.user_timeline(screen_name = screen_name,count=200, tweet_mode='extended')
	
	#save most recent tweets
	alltweets.extend(new_tweets)
	
	#save the id of the oldest tweet less one
	oldest = alltweets[-1].id - 1
	
	#keep grabbing tweets until there are no tweets left to grab
	while len(new_tweets) > 0:
		
		#all subsiquent requests use the max_id param to prevent duplicates
		new_tweets = api.user_timeline(screen_name = screen_name,count=200,max_id=oldest, tweet_mode='extended')
		
		#save most recent tweets
		alltweets.extend(new_tweets)
		
		#update the id of the oldest tweet less one
		oldest = alltweets[-1].id - 1
		
	
	#transform the tweepy tweets into a 2D array that will populate the csv
		outtweets = [[tweet.id_str, tweet.created_at, tweet.full_text] for tweet in alltweets]

		#for item in alltweets:
			#print(item.full_text)
                
	#write the csv	
	with open('%s_tweets.csv' % screen_name, 'w', newline='', encoding='utf-8') as f:
		writer = csv.writer(f)
		writer.writerow(["id","created_at","text"])
		writer.writerows(outtweets)
	
	pass

	print("CSV finished. If opening with excel, use Data-->Import External Data --> Import Data and select UTF-8")
	input("Press Enter to continue...")

if __name__ == '__main__':
	#pass in the username of the account you want to download
	get_all_tweets("hqtriviascribe")
