package SpecFile;
use OTA::Node;
our @ISA = qw(Node);

use strict;
use warnings;

sub new {
	my ($class) = @_;
	my $self = $class->SUPER::new;
	$self->{_filename} = undef;
	$self->{_version} = undef;
	$self->{_has_change_log} = 0;
	$self->{_applied_patches} = ();
	$self->{_referenced_patches} = ();
	bless $self, $class;
	return $self;
}

sub FileName {
	my ($self, $x) = @_;
	$self->{_filename} = $x if $x;
	return $self->{_filename};
}

sub Version {
	my ($self, $x) = @_;
	$self->{_version} = $x if $x;
	return $self->{_version};
}

sub HasChangeLog {
	my ($self, $x) = @_;
	$self->{_has_change_log} = $x if $x;
	return $self->{_has_change_log};
}

sub AppliedPatches {
	my ($self, $x) = @_;
	$self->{_applied_patches} = $x if $x;
	return $self->{_applied_patches};
}

sub ReferencedPatches {
	my ($self, $x) = @_;
	$self->{_referenced_patches} = $x if $x;
	return $self->{_referenced_patches};
}

1;
