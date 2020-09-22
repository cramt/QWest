# frozen_string_literal: true

require 'active_support/core_ext/string/filters'
require 'json'
require 'HTTParty'
require 'unidecoder'
require './deep_string_cmp.rb'
require './wiki_search.rb'
require 'parallel'
countries_str = File.read 'iso3166-1.json'
subdivision_str = File.read 'iso3166-2.json'
countries = JSON.parse(countries_str)['3166-1'].map {|x| x.transform_keys(&:to_sym)}
subdivision = JSON.parse(subdivision_str)['3166-2'].map {|x| x.transform_keys(&:to_sym)}
countries = Parallel.map(countries.map.with_index {|country, i| [country, i]}, in_threads: 10) do |country, i|
    subs = subdivision.select {|sub| sub[:code].start_with?(country[:alpha_2] + '-')}
    subs.map do |sub|
        sub[:code] = sub[:code][3..-1]
        sub[:subdivision] = []
        sub
    end
    subs = Parallel.map(subs, in_threads: 2 ** 64) do |sub|
        names = [sub[:name], sub[:name].to_ascii].uniq
        wiki_data = names.map do |x|
            Utils.search x
        end
        wiki_article_name = names.dup.concat(names.map {|x| x + ' ' + sub[:type]}).concat(names.map {|x| sub[:type] + ' ' + x})
        wiki_article = wiki_data.flatten.map do |wiki|
            [wiki_article_name.map {|n| Utils.deep_string_cmp(n, wiki[:title])}.max, wiki]
        end.max_by {|x| x[0]}
        if wiki_article
            wiki_article = wiki_article[1]
            regex = /(\(:?(:?(?:.*?:)(:?.*?)(?::?))+\))/
            match = wiki_article[:snippet].match regex
            alternative_names = if match
                match.to_s[1..-2].split(';').map do |x|
                    if x.include? ':'
                        split = x.split(':')
                        split.shift
                        split = split.map {|y| y.split(',')}.flatten
                        split.dup.concat(split.map(&:to_ascii))
                    else
                        []
                    end
                end.flatten.concat(names).uniq
            else
                names
            end
            sub[:names] = alternative_names
        else
            sub[:names] = names
        end
        puts "finished #{sub[:name]}"
        sub
    end
    subs = subs.map {|sub| [sub[:code], sub]}.to_h
    subs.each do |key, val|
        next unless val[:parent]

        val[:parent] = val[:parent][3..-1] if val[:parent].start_with?(country[:alpha_2] + '-')
        parent = val[:parent]
        raise 'fuck' unless subs[parent]

        subs.delete key
        val.delete :parent
        subs[parent][:subdivision].push val
    end
    country[:subdivision] = subs.values.to_a
    puts "#{i}/#{countries.length}, #{(i / countries.length.to_f) * 100}% done"
    puts "finished #{country[:name]}"
    country
end
str = countries.to_json
File.write('countries.json', str)
str = str.to_json
File.write('countries_string_ready.json', str)
File.write('ISO3166String.cs', "namespace GeographicSubdivision.Provider { class ISO3166String { internal const string ISO3166 = #{str}; } }")
