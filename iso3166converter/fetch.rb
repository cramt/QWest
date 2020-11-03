# frozen_string_literal: true

require 'nokogiri'
require 'httparty'

module Fetch
    def fetch(alpha2)
        html = Nokogiri.parse(HTTParty.get('https://en.wikipedia.org/api/rest_v1/page/html/ISO_3166-2:' + alpha2).body.to_s)
        tables = html.search('.wikitable')
        map = tables
        map = map.select{|table| !table.parent.children[0].text.start_with?("Codes before")}
        map = map.map do |table|
            entries = table.children[1].children.select {|x| x.name == 'tr'}
            entries.slice!(0)
            entries.map do |entry|
                entry.children.select {|x| x.name == 'td'}.map {|x| x.text}
            end
        end
        map = map.map {|x| x.select {|y| y.length != 0}}
        map = map.select {|y| y.all? {|x| x[0].is_a?(String) && x[0].start_with?(alpha2 + '-')}}
        map = map.map {|x| x.map {|y| y[0] = y[0][3..-1]; y}}
        map = map.inject([]) {|acc, x| acc.concat x}
        map = map.inject({}) {|acc, x|
            alpha_2 = x.slice!(0)
            x = x.map{|y|y.gsub(" ", "")}
            last = x.pop
            acc[alpha_2.to_sym] = {
                :alpha_2 => alpha_2,
                :name => x.first,
                :names => x,
                :subdivisions => [],
                :last_entry => last
            }
            acc
        }
        results = []
        map.values.each {|x|
            if map.key?(x[:last_entry].to_sym)
                sup = map[x[:last_entry].to_sym]
                sup[:subdivisions].push x
                x.delete :last_entry
                map.delete(x[:alpha_2].to_sym)
            else
                x[:names].push x[:last_entry]
                x[:name] = x[:names].first if x[:name] == nil
                x.delete :last_entry
                x[:names] = x[:names].uniq
                results.push x
            end
        }
        if map.values.map{|x|x[:names].last}.group_by{ |e| e }.values.map{|x|x.length}.any?{|x|x > 1}
            map.values.each {|x|
                x[:names].pop
            }
        end
        results
    end

    module_function :fetch
end
